using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByAuthorityAsync(string authority, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByStatusAsync(string status, CancellationToken cancellationToken = default);
}