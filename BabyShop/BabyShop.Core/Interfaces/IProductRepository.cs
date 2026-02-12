using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryIdPagedAsync(int categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductWithInventoryAsync(int id, CancellationToken cancellationToken = default);
}