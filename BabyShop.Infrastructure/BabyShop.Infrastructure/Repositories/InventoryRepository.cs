using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.ProductId == productId && !i.IsDeleted);
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
            .Where(i => i.CurrentStock < i.MinimumStockLevel && !i.IsDeleted)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Inventory>> GetOutOfStockAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
            .Where(i => i.CurrentStock == 0 && !i.IsDeleted)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Inventory>> GetNeedsReorderAsync()
    {
        return await _dbSet
            .Include(i => i.Product)
            .Where(i => i.CurrentStock <= i.ReorderPoint && !i.IsDeleted)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync();
    }
}