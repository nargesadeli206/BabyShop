using BabyShop.BabyShop.Core.Entities;
using System.Threading.Tasks;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentEntity> CreatePaymentAsync(int orderId, decimal amount)
    {
        var payment = new PaymentEntity(orderId, amount);
        return await _paymentRepository.AddAsync(payment);
    }

    public async Task<bool> VerifyAsync(string authority)
    {
        var payment = await _paymentRepository.GetByAuthorityAsync(authority);
        if (payment == null) return false;

        payment.MarkAsPaid();
        await _paymentRepository.AddAsync(payment); 
        return true;
    }
}