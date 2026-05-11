using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using BabyShop.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessRuleException("Product name is required");

        var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);
        if (!categoryExists)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        var gender = Gender.FromValue(dto.Gender);
        var ageRange = AgeRange.FromCode(dto.AgeRange);

        var product = new Product(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.CategoryId,
            gender,
            ageRange
        );

        var createdProduct = await _productRepository.AddAsync(product);

        _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);

        return MapToDto(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(dto.Id);
        if (existingProduct == null)
            throw new NotFoundException(nameof(Product), dto.Id);

        var categoryExists = await _productRepository.CategoryExistsAsync(dto.CategoryId);
        if (!categoryExists)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        var gender = Gender.FromValue(dto.Gender);
        var ageRange = AgeRange.FromCode(dto.AgeRange);

        existingProduct.Update(
            dto.Name,
            dto.Description,
            dto.Price,
            gender,
            ageRange
        );

        await _productRepository.UpdateAsync(existingProduct);

        _logger.LogInformation("Product updated with ID: {ProductId}", existingProduct.Id);

        return MapToDto(existingProduct);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);
        if (product == null) return null;
        return MapToDto(product);
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllWithCategoryAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<PagedResultDto<ProductListDto>> GetProductsPagedAsync(
        int pageNumber, int pageSize, int? gender = null, string? ageRange = null)
    {
        var totalCount = await _productRepository.GetProductsCountAsync(
            categoryId: null,
            gender: gender,
            ageRange: ageRange,
            searchTerm: null);

        var products = await _productRepository.GetProductsAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            categoryId: null,
            gender: gender,
            ageRange: ageRange,
            searchTerm: null);

        return new PagedResultDto<ProductListDto>
        {
            Items = products.Select(MapToListDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return products.Select(MapToDto).ToList();
    }

    public async Task<List<ProductDto>> SearchProductsAsync(string term)
    {
        var products = await _productRepository.SearchAsync(term);
        return products.Select(MapToDto).ToList();
    }

    public async Task DeleteProductAsync(int id)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            throw new NotFoundException(nameof(Product), id);

        await _productRepository.DeleteProductAsync(id);

        _logger.LogInformation("Product deleted with ID: {ProductId}", id);
    }

    public async Task<List<ProductDto>> GetProductsByGenderAsync(int gender)
    {
        var genderValue = Gender.FromValue(gender);
        var products = await _productRepository.GetByGenderAsync(genderValue);
        return products.Select(MapToDto).ToList();
    }

    public async Task<List<ProductDto>> GetProductsByAgeRangeAsync(string ageRange)
    {
        var ageRangeValue = AgeRange.FromCode(ageRange);
        var products = await _productRepository.GetByAgeRangeAsync(ageRangeValue);
        return products.Select(MapToDto).ToList();
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            IsActive = product.IsActive,
            ViewCount = product.ViewCount,
            SoldCount = product.SoldCount,
            CreatedAt = product.CreatedAt,
            GenderValue = product.Gender?.Value ?? 0,
            GenderName = product.Gender?.DisplayName ?? string.Empty,
            AgeRangeCode = product.AgeRange?.Code ?? string.Empty,
            AgeRangeDisplay = product.AgeRange?.DisplayName ?? string.Empty,
            CurrentStock = product.StockQuantity
        };
    }

    private ProductListDto MapToListDto(Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Price = product.Price,
            CategoryName = product.Category?.Name ?? string.Empty,
            IsActive = product.IsActive,
            SoldCount = product.SoldCount,
            GenderName = product.Gender?.DisplayName ?? string.Empty,
            AgeRangeDisplay = product.AgeRange?.DisplayName ?? string.Empty,
            CurrentStock = product.StockQuantity
        };
    }
}