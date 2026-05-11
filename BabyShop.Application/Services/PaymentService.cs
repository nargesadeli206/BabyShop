using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null) return null;
        return MapToDto(payment);
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
    {
        var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
        if (payment == null) return null;
        return MapToDto(payment);
    }

    public async Task<PaymentDto?> GetPaymentByAuthorityAsync(string authority)
    {
        var payment = await _paymentRepository.GetPaymentByAuthorityAsync(authority);
        if (payment == null) return null;
        return MapToDto(payment);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), dto.OrderId);

        // ✅ درست: استفاده از سازنده
        var payment = new Payment(dto.OrderId, dto.Amount, dto.PaymentMethod);

        var createdPayment = await _paymentRepository.CreatePaymentAsync(payment);

        _logger.LogInformation("Payment created for order {OrderId} with amount {Amount}",
            dto.OrderId, dto.Amount);

        return MapToDto(createdPayment);
    }

    public async Task<PaymentDto> VerifyPaymentAsync(int paymentId, string referenceNumber)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException(nameof(Payment), paymentId);

        // ✅ درست: استفاده از متد Verify
        payment.Verify(referenceNumber);
        await _paymentRepository.UpdateAsync(payment);

        // به‌روزرسانی وضعیت سفارش
        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.MarkAsPaid();
            await _orderRepository.UpdateAsync(order);
        }

        _logger.LogInformation("Payment {PaymentId} verified with reference {ReferenceNumber}",
            paymentId, referenceNumber);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> RefundPaymentAsync(int paymentId, string reason)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException(nameof(Payment), paymentId);

        // ✅ درست: استفاده از متد Refund
        payment.Refund(reason);
        await _paymentRepository.UpdateAsync(payment);

        // به‌روزرسانی وضعیت سفارش
        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.MarkAsRefunded();
            await _orderRepository.UpdateAsync(order);
        }

        _logger.LogInformation("Payment {PaymentId} refunded. Reason: {Reason}",
            paymentId, reason);

        return MapToDto(payment);
    }

    public async Task<decimal> GetOrderTotalAmountAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), orderId);
        return order.TotalAmount;
    }

    private PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status,
            Authority = payment.Authority,
            ReferenceNumber = payment.ReferenceNumber,
            PaidAt = payment.PaidAt,
            RefundedAt = payment.RefundedAt,
            RefundReason = payment.RefundReason,
            PaymentMethod = payment.PaymentMethod,
            CreatedAt = payment.CreatedAt
        };
    }
}