using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class DeliveryRepository : Repository<Delivery>, IDeliveryRepository
{
    public DeliveryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Delivery?> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.OrderId == orderId && !d.IsDeleted);
    }

    public async Task<IReadOnlyList<Delivery>> GetPendingDeliveriesAsync()
    {
        return await _dbSet
            .Where(d => (d.Status == "Pending" || d.Status == "Processing") && !d.IsDeleted)
            .OrderBy(d => d.EstimatedDeliveryDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Delivery>> GetDeliveriesByStatusAsync(string status)
    {
        return await _dbSet
            .Where(d => d.Status == status && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}