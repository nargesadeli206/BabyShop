using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetProductWithInventoryAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }
}