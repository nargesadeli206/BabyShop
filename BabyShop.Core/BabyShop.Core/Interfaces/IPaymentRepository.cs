using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByAuthorityAsync(string authority);
    Task<IReadOnlyList<Payment>> GetPaymentsByStatusAsync(string status);
}