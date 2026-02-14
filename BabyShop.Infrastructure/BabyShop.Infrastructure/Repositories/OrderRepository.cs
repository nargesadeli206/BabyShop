using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Delivery)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId)
    {
        return await _dbSet
            .Where(o => o.UserId == userId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status)
    {
        return await _dbSet
            .Where(o => o.Status == status && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}