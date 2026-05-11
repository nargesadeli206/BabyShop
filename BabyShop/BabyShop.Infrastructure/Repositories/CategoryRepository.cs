using Microsoft.EntityFrameworkCore;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;

namespace BabyShop.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    // Check if slug is unique
    public async Task<bool> IsSlugUniqueAsync(string slug, int? id = null)
    {
        if (id.HasValue)
        {
            return !await _dbSet.AnyAsync(c => c.Slug == slug && c.Id != id && !c.IsDeleted);
        }
        return !await _dbSet.AnyAsync(c => c.Slug == slug && !c.IsDeleted);
    }

    // Get category with subcategories
    public async Task<Category?> GetCategoryWithSubCategoriesAsync(int id)
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    // Get category with products
    public async Task<Category?> GetCategoryWithProductsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    // Get main categories (no parent)
    public async Task<List<Category>> GetMainCategoriesAsync()
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    // Get subcategories of a parent
    public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    // Check if category has products
    public async Task<bool> HasProductsAsync(int id)
    {
        return await _context.Set<Product>()
            .AnyAsync(p => p.CategoryId == id && !p.IsDeleted);
    }

    // Check if category has subcategories
    public async Task<bool> HasSubCategoriesAsync(int id)
    {
        return await _dbSet.AnyAsync(c => c.ParentCategoryId == id && !c.IsDeleted);
    }

    // Get queryable for paging
    public async Task<IQueryable<Category>> GetQueryableAsync()
    {
        return _dbSet
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .Where(c => !c.IsDeleted)
            .AsQueryable();
    }

    // Count for paging
    public async Task<int> CountAsync(IQueryable<Category> query)
    {
        return await query.CountAsync();
    }

    // Get paged results
    public async Task<List<Category>> GetPagedAsync(IQueryable<Category> query, int pageNumber, int pageSize)
    {
        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}