using BabyShop.BabyShop.Core.Entities;
using BabyShop.BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    public PaymentRepository(AppDbContext context) => _context = context;

    public async Task<PaymentEntity> AddAsync(PaymentEntity payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<PaymentEntity?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<PaymentEntity?> GetByAuthorityAsync(string authority)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.Authority == authority);
    }
}