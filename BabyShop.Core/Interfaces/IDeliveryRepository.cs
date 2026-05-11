using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IDeliveryRepository
{
    // متدهای پایه
    Task<Delivery?> GetByIdAsync(int id);
    Task<IReadOnlyList<Delivery>> GetAllAsync();
    Task<Delivery> AddAsync(Delivery entity);
    Task UpdateAsync(Delivery entity);
    Task DeleteAsync(Delivery entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();

    // متدهای اختصاصی Delivery
    Task<Delivery?> GetByOrderIdAsync(int orderId);
    Task<List<Delivery>> GetPendingDeliveriesAsync();
    Task<Delivery> CreateDeliveryAsync(Delivery delivery);
    Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, string status, string? trackingNumber);
    Task<bool> IsOrderPaidAsync(int orderId);
}