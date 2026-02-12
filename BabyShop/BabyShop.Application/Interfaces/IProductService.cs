using BabyShop.Application.Dtos.Common;
using BabyShop.Application.Dtos.Products;

namespace BabyShop.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateProductAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ProductSummaryResponse>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ProductSummaryResponse>> GetProductsByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductSummaryResponse>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ProductExistsAsync(int id, CancellationToken cancellationToken = default);
}