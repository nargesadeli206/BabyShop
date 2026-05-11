using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(int id);
    Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId);
    Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status);
    Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Order>> GetOldDeliveredOrdersAsync(DateTime olderThan);

    Task<List<Order>> GetAbandonedCartsAsync(DateTime olderThan);
}