using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;

namespace BabyShop.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> GetCategoryDtoByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return null;

        return MapToDto(category);
    }

    public async Task<CategoryDto?> GetCategoryDtoBySlugAsync(string slug)
    {
        var categories = await _categoryRepository.FindAsync(c => c.Slug == slug);
        var category = categories.FirstOrDefault();
        if (category == null) return null;

        return MapToDto(category);
    }

    public async Task<List<CategoryDto>> GetAllCategoryDtosAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetMainCategoryDtosAsync()
    {
        var categories = await _categoryRepository.GetMainCategoriesAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetSubCategoryDtosAsync(int parentId)
    {
        var categories = await _categoryRepository.FindAsync(c => c.ParentCategoryId == parentId);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<BabyShop.Application.Dtos.PagedResultDto<CategoryDto>> GetCategoriesPagedAsync(int pageNumber, int pageSize)
    {
        var categories = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);
        var totalCount = await _categoryRepository.CountAsync();

        return new BabyShop.Application.Dtos.PagedResultDto<CategoryDto>
        {
            Items = categories.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category(
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.ImageUrl,
            dto.DisplayOrder
        );

        var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
        return MapToDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.Id);
        if (category == null)
            throw new Exception("Category not found");

        category.Update(dto.Name, dto.Description, dto.ImageUrl, dto.DisplayOrder);

        if (!dto.IsActive)
            category.Deactivate();
        else
            category.Activate();

        await _categoryRepository.UpdateAsync(category);
        return MapToDto(category);
    }

    public async Task SoftDeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category != null)
        {
            category.Deactivate();
            await _categoryRepository.UpdateAsync(category);
        }
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await _categoryRepository.HasProductsAsync(id);
    }

    public async Task<bool> HasSubCategoriesAsync(int id)
    {
        return await _categoryRepository.HasSubCategoriesAsync(id);
    }

    private CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            SubCategories = category.SubCategories?.Select(MapToDto).ToList() ?? new List<CategoryDto>()
        };
    }
}