using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IOrderRepository
{
    // متدهای پایه
    Task<Order?> GetByIdAsync(int id);
    Task<IReadOnlyList<Order>> GetAllAsync();
    Task<Order> AddAsync(Order entity);
    Task UpdateAsync(Order entity);
    Task DeleteAsync(Order entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();

    // متدهای اختصاصی Order
    Task<Order?> GetOrderWithItemsAsync(int id);
    Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId);
    Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status);
    Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Order>> GetOldDeliveredOrdersAsync(DateTime olderThan);
    Task<List<Order>> GetAbandonedCartsAsync(DateTime olderThan);

    // متد کمکی برای جستجو
    Task<IReadOnlyList<Order>> FindAsync(Func<Order, bool> predicate);
}