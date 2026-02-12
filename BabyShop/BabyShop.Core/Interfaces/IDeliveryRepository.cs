using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IDeliveryRepository : IRepository<Delivery>
{
    Task<Delivery?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Delivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken = default);
}