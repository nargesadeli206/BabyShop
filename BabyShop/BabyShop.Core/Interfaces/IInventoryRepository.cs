using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<Inventory>> GetLowStockAsync();
    Task<IReadOnlyList<Inventory>> GetOutOfStockAsync();
    Task<IReadOnlyList<Inventory>> GetNeedsReorderAsync();
}