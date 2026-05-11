using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
    Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
    Task<PaymentDto?> GetPaymentByAuthorityAsync(string authority);
    Task<bool> VerifyPaymentAsync(VerifyPaymentDto dto);
    Task<bool> RefundPaymentAsync(int paymentId);
}