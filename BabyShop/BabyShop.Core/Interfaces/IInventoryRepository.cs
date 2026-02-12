using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inventory>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inventory>> GetOutOfStockAsync(CancellationToken cancellationToken = default);
}