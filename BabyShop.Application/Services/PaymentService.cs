using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
using System.Threading.Tasks;
using System;

namespace BabyShop.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
        }

        // ۱. ایجاد تراکنش پرداخت جدید (متصل به درگاه)
        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null) throw new NotFoundException("سفارش مورد نظر یافت نشد.");

            // جلوگیری از پرداختِ سفارشی که قبلاً پرداخت شده
            if (order.Status == "Paid")
                throw new BusinessRuleException("این سفارش قبلاً پرداخت شده است.");

            // ایجاد آبجکت Payment برای دیتابیس
            var payment = new Payment(dto.OrderId, dto.Amount, dto.PaymentMethod);

            // تولید یه کد آتوریتی رندوم (شبیه سازی بازگشت از درگاه شاپرک)
            payment.SetAuthority($"ZIIZII-{Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()}");

            var createdPayment = await _paymentRepository.AddAsync(payment);
            return MapToDto(createdPayment);
        }

        // ۲. تایید نهایی پرداخت (هنگام برگشت از درگاه بانک)
        public async Task<PaymentDto> VerifyPaymentAsync(int paymentId, string referenceNumber)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) throw new NotFoundException("تراکنش یافت نشد.", paymentId);

            if (payment.Status == "Successful")
                throw new BusinessRuleException("این تراکنش قبلاً تایید شده است.");

            // تایید پرداخت و ثبت شماره پیگیری بانک
            payment.Verify(referenceNumber);
            await _paymentRepository.UpdateAsync(payment);

            // حالا باید وضعیت سفارش اصلی رو هم بکنیم 'پرداخت شده'
            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.MarkAsPaid();
                await _orderRepository.UpdateAsync(order);
            }

            return MapToDto(payment);
        }

        public async Task<PaymentDto> RefundPaymentAsync(int paymentId, string reason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) throw new NotFoundException("Payment", paymentId);

            payment.Refund(reason);
            await _paymentRepository.UpdateAsync(payment);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.MarkAsRefunded();
                await _orderRepository.UpdateAsync(order);
            }

            return MapToDto(payment);
        }

        public async Task<decimal> GetOrderTotalAmountAsync(int orderId) =>
            (await _orderRepository.GetByIdAsync(orderId))?.TotalAmount ?? 0;

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id) =>
            MapToDto(await _paymentRepository.GetByIdAsync(id));

        public async Task<PaymentDto?> GetPaymentByAuthorityAsync(string auth) =>
            MapToDto(await _paymentRepository.GetPaymentByAuthorityAsync(auth));

        public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int id) =>
            MapToDto(await _paymentRepository.GetPaymentByOrderIdAsync(id));

        private PaymentDto MapToDto(Payment? p)
        {
            if (p == null) return null!;
            return new PaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Status = p.Status,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Authority = p.Authority,
                ReferenceNumber = p.ReferenceNumber
            };
        }
    }
}