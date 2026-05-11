using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.OrderId == orderId && !p.IsDeleted);
    }

    public async Task<Payment?> GetByAuthorityAsync(string authority)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Authority == authority && !p.IsDeleted);
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByStatusAsync(string status)
    {
        return await _dbSet
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}