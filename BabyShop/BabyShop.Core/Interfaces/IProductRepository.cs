using BabyShop.Core.Entities;

namespace BabyShop.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithTrackingAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<bool> ExistsAsync(int id);
    Task<int> GetStockAsync(int productId);
}