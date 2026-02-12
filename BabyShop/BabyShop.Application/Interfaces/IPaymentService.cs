using BabyShop.BabyShop.Core.Entities;
using System.Threading.Tasks;

public interface IPaymentService
{
    Task<PaymentEntity> CreatePaymentAsync(int orderId, decimal amount);
    Task<bool> VerifyAsync(string authority);
}