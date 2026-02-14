using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        ICategoryRepository categoryRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessRuleException("Product name is required");
        if (dto.Price <= 0)
            throw new BusinessRuleException("Product price must be greater than zero");

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        var product = new Product(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? "",
            dto.Price,
            dto.CategoryId);

        await _productRepository.AddAsync(product);

        if (dto.InitialStock > 0)
        {
            var inventory = new Inventory(product.Id, dto.InitialStock);
            await _inventoryRepository.AddAsync(inventory);
        }

        _logger.LogInformation("Product created with ID: {ProductId}", product.Id);
        return await MapToProductDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
            throw new NotFoundException(nameof(Product), dto.Id);

        product.Update(dto.Name, dto.Description, dto.Price);
        await _productRepository.UpdateAsync(product);

        _logger.LogInformation("Product updated with ID: {ProductId}", product.Id);
        return await MapToProductDto(product);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetProductWithInventoryAsync(id);
        if (product == null || product.IsDeleted)
            return null;

        product.IncrementViewCount();
        await _productRepository.UpdateAsync(product);

        return await MapToProductDto(product);
    }

    public async Task<PagedResultDto<ProductListDto>> GetProductsPagedAsync(int pageNumber, int pageSize)
    {
        var products = await _productRepository.GetPagedAsync(pageNumber, pageSize);
        var totalCount = await _productRepository.CountAsync();
        var items = new List<ProductListDto>();

        foreach (var product in products.Where(p => !p.IsDeleted))
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(product.Id);
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            items.Add(new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = inventory?.CurrentStock ?? 0,
                CategoryName = category?.Name ?? "",
                IsActive = product.IsActive
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
        var products = await _productRepository.GetByCategoryIdAsync(categoryId);
        var result = new List<ProductDto>();

        foreach (var product in products.Where(p => !p.IsDeleted && p.IsActive))
        {
            result.Add(await MapToProductDto(product));
        }

        return result;
    }

    public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<ProductDto>();

        var products = await _productRepository.SearchProductsAsync(searchTerm);
        var result = new List<ProductDto>();

        foreach (var product in products.Where(p => !p.IsDeleted && p.IsActive))
        {
            result.Add(await MapToProductDto(product));
        }

        return result;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException(nameof(Product), id);

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _productRepository.UpdateAsync(product);

        _logger.LogInformation("Product deleted with ID: {ProductId}", id);
    }

    private async Task<ProductDto> MapToProductDto(Product product)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(product.Id);
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = inventory?.CurrentStock ?? 0,
            CategoryName = category?.Name ?? "",
            IsActive = product.IsActive,
            ViewCount = product.ViewCount,
            SoldCount = product.SoldCount,
            CreatedAt = product.CreatedAt
        };
    }

    public Task<List<ProductDto>> GetAllProductsAsync()
    {
        throw new NotImplementedException();
    }
}