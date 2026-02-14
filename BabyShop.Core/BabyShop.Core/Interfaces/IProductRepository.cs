using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProductWithInventoryAsync(int id);
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync();
    Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm);
}