using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetCategoryWithSubCategoriesAsync(int id)
    {
        return await _dbSet
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IReadOnlyList<Category>> GetMainCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Slug == slug && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}