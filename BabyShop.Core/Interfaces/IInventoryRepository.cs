using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IInventoryRepository
{
    // متدهای پایه
    Task<Inventory?> GetByIdAsync(int id);
    Task<IReadOnlyList<Inventory>> GetAllAsync();
    Task<Inventory> AddAsync(Inventory entity);
    Task UpdateAsync(Inventory entity);
    Task DeleteAsync(Inventory entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();

    // متدهای اختصاصی Inventory
    Task<Inventory?> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<Inventory>> GetAllWithProductAsync();
    Task<IReadOnlyList<Inventory>> GetLowStockInventoriesAsync();
    Task DecreaseStockAsync(int productId, int quantity);
    Task IncreaseStockAsync(int productId, int quantity);
}