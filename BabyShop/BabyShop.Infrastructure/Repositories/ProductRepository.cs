using Microsoft.EntityFrameworkCore;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.ValueObjects;
using BabyShop.Infrastructure.Data;

namespace BabyShop.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    // این خط رو اصلاح کردم: } به { تبدیل شد
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<List<Product>> GetAllWithCategoryAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchAsync(string term)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Name.Contains(term) || p.Description.Contains(term))
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByGenderAsync(Gender gender)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Where(p => p.Gender.Equals(gender) && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByAgeRangeAsync(AgeRange ageRange)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Where(p => p.AgeRange.Equals(ageRange) && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IQueryable<Product>> GetQueryableAsync()
    {
        return _dbSet
            .Include(p => p.Category)
            .Include(p => p.Inventory)
            .Where(p => !p.IsDeleted)
            .AsQueryable();
    }

    public async Task<int> CountAsync(IQueryable<Product> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<Product>> GetPagedAsync(IQueryable<Product> query, int pageNumber, int pageSize)
    {
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }


    public async Task<List<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _dbSet
            .Include(p => p.Inventory)
            .Where(p => !p.IsDeleted && p.Inventory != null && p.Inventory.CurrentStock <= threshold)
            .ToListAsync();
    }
}