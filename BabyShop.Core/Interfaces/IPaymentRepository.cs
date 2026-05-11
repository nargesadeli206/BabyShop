using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IPaymentRepository
{
    // متدهای پایه
    Task<Payment?> GetByIdAsync(int id);
    Task<IReadOnlyList<Payment>> GetAllAsync();
    Task<Payment> AddAsync(Payment entity);
    Task UpdateAsync(Payment entity);
    Task DeleteAsync(Payment entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();

    // متدهای اختصاصی Payment
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task<Payment?> GetPaymentByAuthorityAsync(string authority);
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Payment> UpdatePaymentAsync(Payment payment);
    Task<Payment> VerifyPaymentAsync(int paymentId, string referenceNumber);
    Task<Payment> RefundPaymentAsync(int paymentId, string reason);
    Task<decimal> GetOrderTotalAmountAsync(int orderId);
}