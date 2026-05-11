using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IDeliveryRepository : IRepository<Delivery>
{
    Task<Delivery?> GetByOrderIdAsync(int orderId);
    Task<IReadOnlyList<Delivery>> GetPendingDeliveriesAsync();
    Task<IReadOnlyList<Delivery>> GetDeliveriesByStatusAsync(string status);
}