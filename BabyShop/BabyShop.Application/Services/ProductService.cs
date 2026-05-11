using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using BabyShop.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IInventoryRepository inventoryRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // اعتبارسنجی
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessRuleException("Product name is required");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        // تبدیل Gender و AgeRange از DTO به ValueObject
        Gender gender;
        AgeRange ageRange;

        try
        {
            gender = Gender.FromInt(dto.Gender);
            ageRange = AgeRange.FromCode(dto.AgeRange);
        }
        catch (DomainException ex)
        {
            throw new BusinessRuleException($"Invalid product attributes: {ex.Message}");
        }

        // ساخت محصول جدید
        var product = new Product(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? "",
            dto.Price,
            dto.CategoryId,
            gender,
            ageRange
        );

        await _productRepository.AddAsync(product);

        // اضافه کردن موجودی اولیه
        if (dto.InitialStock > 0)
        {
            var inventory = new Inventory(product.Id, dto.InitialStock);
            await _inventoryRepository.AddAsync(inventory);
        }

        _logger.LogInformation("Product created with ID: {ProductId}", product.Id);

        return await MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
            throw new NotFoundException(nameof(Product), dto.Id);

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        // تبدیل Gender و AgeRange
        Gender gender;
        AgeRange ageRange;

        try
        {
            gender = Gender.FromInt(dto.Gender);
            ageRange = AgeRange.FromCode(dto.AgeRange);
        }
        catch (DomainException ex)
        {
            throw new BusinessRuleException($"Invalid product attributes: {ex.Message}");
        }

        // آپدیت محصول
        product.Update(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? "",
            dto.Price,
            gender,
            ageRange
        );

        await _productRepository.UpdateAsync(product);
        _logger.LogInformation("Product updated with ID: {ProductId}", product.Id);

        return await MapToDto(product);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);
        if (product == null)
            return null;

        return await MapToDto(product);
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllWithCategoryAsync();
        var dtos = new List<ProductDto>();

        foreach (var product in products)
        {
            dtos.Add(await MapToDto(product));
        }

        return dtos;
    }

    public async Task<PagedResultDto<ProductListDto>> GetProductsPagedAsync(
        int pageNumber, int pageSize, int? gender = null, string? ageRange = null)
    {
        var query = await _productRepository.GetQueryableAsync();

        // اعمال فیلترها
        if (gender.HasValue)
        {
            var genderValue = Gender.FromInt(gender.Value);
            query = query.Where(p => p.Gender.Equals(genderValue));
        }

        if (!string.IsNullOrEmpty(ageRange))
        {
            var ageRangeValue = AgeRange.FromCode(ageRange);
            query = query.Where(p => p.AgeRange.Equals(ageRangeValue));
        }

        var totalCount = await _productRepository.CountAsync(query);
        var products = await _productRepository.GetPagedAsync(query, pageNumber, pageSize);

        var items = new List<ProductListDto>();
        foreach (var product in products)
        {
            items.Add(new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                CategoryName = product.Category?.Name ?? "",
                GenderName = product.Gender.Name,
                AgeRangeDisplay = product.AgeRange.DisplayName,
                IsActive = product.IsActive,
                SoldCount = product.SoldCount,
                CurrentStock = product.Inventory?.CurrentStock ?? 0
            });
        }

        return new PagedResultDto<ProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        var dtos = new List<ProductDto>();

        foreach (var product in products)
        {
            dtos.Add(await MapToDto(product));
        }

        return dtos;
    }

    public async Task<List<ProductDto>> SearchProductsAsync(string term)
    {
        var products = await _productRepository.SearchAsync(term);
        var dtos = new List<ProductDto>();

        foreach (var product in products)
        {
            dtos.Add(await MapToDto(product));
        }

        return dtos;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        await _productRepository.DeleteAsync(product);
        _logger.LogInformation("Product deleted with ID: {ProductId}", id);
    }

    public async Task<List<ProductDto>> GetProductsByGenderAsync(int gender)
    {
        var genderValue = Gender.FromInt(gender);
        var products = await _productRepository.GetByGenderAsync(genderValue);
        var dtos = new List<ProductDto>();

        foreach (var product in products)
        {
            dtos.Add(await MapToDto(product));
        }

        return dtos;
    }

    public async Task<List<ProductDto>> GetProductsByAgeRangeAsync(string ageRange)
    {
        var ageRangeValue = AgeRange.FromCode(ageRange);
        var products = await _productRepository.GetByAgeRangeAsync(ageRangeValue);
        var dtos = new List<ProductDto>();

        foreach (var product in products)
        {
            dtos.Add(await MapToDto(product));
        }

        return dtos;
    }

    // متد کمکی برای تبدیل Product به ProductDto
    private async Task<ProductDto> MapToDto(Product product)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(product.Id);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "",
            GenderValue = product.Gender.Value,
            GenderName = product.Gender.Name,
            AgeRangeCode = product.AgeRange.Code,
            AgeRangeDisplay = product.AgeRange.DisplayName,
            IsActive = product.IsActive,
            ViewCount = product.ViewCount,
            SoldCount = product.SoldCount,
            CurrentStock = inventory?.CurrentStock ?? 0
        };
    }
}