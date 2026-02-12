using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default);
}