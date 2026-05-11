using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
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

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), dto.OrderId);

        var existing = await _paymentRepository.GetByOrderIdAsync(dto.OrderId);
        if (existing != null)
            throw new BusinessRuleException($"Payment already exists for order {dto.OrderId}");

        if (dto.Amount <= 0)
            throw new BusinessRuleException("Payment amount must be greater than zero");
        if (dto.Amount != order.TotalAmount)
            throw new BusinessRuleException($"Payment amount must equal order total ({order.TotalAmount})");

        var payment = new Payment(dto.OrderId, dto.Amount);
        await _paymentRepository.AddAsync(payment);

        _logger.LogInformation("Payment created with Authority: {Authority} for Order: {OrderId}",
            payment.Authority, dto.OrderId);

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Authority = payment.Authority,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment == null || payment.IsDeleted)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Authority = payment.Authority,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<PaymentDto?> GetPaymentByAuthorityAsync(string authority)
    {
        var payment = await _paymentRepository.GetByAuthorityAsync(authority);
        if (payment == null || payment.IsDeleted)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Authority = payment.Authority,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task<bool> VerifyPaymentAsync(VerifyPaymentDto dto)
    {
        var payment = await _paymentRepository.GetByAuthorityAsync(dto.Authority);
        if (payment == null)
            throw new NotFoundException(nameof(Payment), dto.Authority);

        if (payment.Status != "Pending")
            throw new BusinessRuleException($"Payment is already {payment.Status}");

        // Simulate payment gateway verification
        payment.MarkAsPaid(Guid.NewGuid().ToString("N"));
        await _paymentRepository.UpdateAsync(payment);

        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.MarkAsPaid();
            await _orderRepository.UpdateAsync(order);
        }

        _logger.LogInformation("Payment verified for Authority: {Authority}, Order: {OrderId}",
            dto.Authority, payment.OrderId);

        return true;
    }

    public async Task<bool> RefundPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException(nameof(Payment), paymentId);

        if (payment.Status != "Paid")
            throw new BusinessRuleException("Only paid payments can be refunded");

        payment.MarkAsRefunded();
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation("Payment refunded for ID: {PaymentId}", paymentId);
        return true;
    }
}