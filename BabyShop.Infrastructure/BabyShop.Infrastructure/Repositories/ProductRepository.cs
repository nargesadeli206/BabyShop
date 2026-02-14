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
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _dbSet
            .Where(p => !p.IsDeleted && p.IsActive &&
                (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}