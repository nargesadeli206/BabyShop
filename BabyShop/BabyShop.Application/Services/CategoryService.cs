using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    // ================ Create ================
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessRuleException("Category name is required");

        var slugUnique = await _categoryRepository.IsSlugUniqueAsync(dto.Name.ToLower().Replace(" ", "-"));
        if (!slugUnique)
            throw new BusinessRuleException("Category with this name already exists");

        var category = new Category(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? "",
            dto.ParentCategoryId,
            dto.ImageUrl,
            dto.DisplayOrder);

        await _categoryRepository.AddAsync(category);
        _logger.LogInformation("Category created with ID: {CategoryId}", category.Id);

        return await MapToCategoryDto(category);
    }

    // ================ Update ================
    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.Id);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.Id);

        var slugUnique = await _categoryRepository.IsSlugUniqueAsync(dto.Name.ToLower().Replace(" ", "-"), dto.Id);
        if (!slugUnique)
            throw new BusinessRuleException("Category with this name already exists");

        category.Update(dto.Name, dto.Description, dto.ImageUrl, dto.DisplayOrder);
        await _categoryRepository.UpdateAsync(category);

        _logger.LogInformation("Category updated with ID: {CategoryId}", category.Id);
        return await MapToCategoryDto(category);
    }

    // ================ Get Single ================
    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryWithSubCategoriesAsync(id);
        if (category == null || category.IsDeleted)
            return null;

        return await MapToCategoryDto(category);
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var category = categories.FirstOrDefault(c => c.Slug == slug && !c.IsDeleted);

        if (category == null)
            return null;

        return await MapToCategoryDto(category);
    }

    // ================ Get Lists ================
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var result = new List<CategoryDto>();

        foreach (var category in categories.Where(c => !c.IsDeleted))
        {
            result.Add(await MapToCategoryDto(category));
        }

        return result;
    }

    public async Task<List<CategoryDto>> GetMainCategoriesAsync()
    {
        var categories = await _categoryRepository.GetMainCategoriesAsync();
        var result = new List<CategoryDto>();

        foreach (var category in categories.Where(c => !c.IsDeleted && c.IsActive))
        {
            result.Add(await MapToCategoryDto(category));
        }

        return result;
    }

    public async Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var subCategories = categories.Where(c => c.ParentCategoryId == parentId && !c.IsDeleted && c.IsActive).ToList();

        var result = new List<CategoryDto>();
        foreach (var category in subCategories)
        {
            result.Add(await MapToCategoryDto(category));
        }

        return result;
    }

    // ================ Paged ================
    public async Task<PagedResultDto<CategoryDto>> GetCategoriesPagedAsync(int pageNumber, int pageSize)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        var activeCategories = allCategories.Where(c => !c.IsDeleted).ToList();

        var totalCount = activeCategories.Count;
        var pagedCategories = activeCategories
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<CategoryDto>();
        foreach (var category in pagedCategories)
        {
            items.Add(await MapToCategoryDto(category));
        }

        return new PagedResultDto<CategoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // ================ Delete ================
    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException(nameof(Category), id);

        // Check if has subcategories
        var hasSubs = await HasSubCategoriesAsync(id);
        if (hasSubs)
            throw new BusinessRuleException("Cannot delete category that has subcategories");

        // Check if has products
        var hasProducts = await HasProductsAsync(id);
        if (hasProducts)
            throw new BusinessRuleException("Cannot delete category that has products");

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _categoryRepository.UpdateAsync(category);

        _logger.LogInformation("Category deleted with ID: {CategoryId}", id);
    }

    // ================ Check Methods ================
    public async Task<bool> HasProductsAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryWithProductsAsync(id);
        return category?.Products?.Any(p => !p.IsDeleted) ?? false;
    }

    public async Task<bool> HasSubCategoriesAsync(int id)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Any(c => c.ParentCategoryId == id && !c.IsDeleted);
    }

    // ================ Private Helper Methods ================
    private async Task<CategoryDto> MapToCategoryDto(Category category)
    {
        var parentCategory = category.ParentCategoryId.HasValue
            ? await _categoryRepository.GetByIdAsync(category.ParentCategoryId.Value)
            : null;

        var subCategories = new List<CategoryDto>();
        var allCategories = await _categoryRepository.GetAllAsync();
        var directSubs = allCategories.Where(c => c.ParentCategoryId == category.Id && !c.IsDeleted).ToList();

        foreach (var sub in directSubs)
        {
            subCategories.Add(await MapToCategoryDto(sub));
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategory?.Name ?? "",
            ImageUrl = category.ImageUrl ?? "",
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ProductsCount = category.Products?.Count(p => !p.IsDeleted) ?? 0,
            SubCategories = subCategories
        };
    }
}