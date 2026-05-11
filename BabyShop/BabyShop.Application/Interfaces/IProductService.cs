using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> UpdateProductAsync(UpdateProductDto dto);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<PagedResultDto<ProductListDto>> GetProductsPagedAsync(int pageNumber, int pageSize, int? gender = null, string? ageRange = null);
    Task<List<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<List<ProductDto>> SearchProductsAsync(string term);
    Task DeleteProductAsync(int id);
    Task<List<ProductDto>> GetProductsByGenderAsync(int gender);
    Task<List<ProductDto>> GetProductsByAgeRangeAsync(string ageRange);
}