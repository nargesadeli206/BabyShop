using BabyShop.Core.Entities;
using System.Linq.Expressions;

namespace BabyShop.Core.Interfaces;

public interface ICategoryRepository
{
    // متدهای پایه
    Task<Category?> GetByIdAsync(int id);
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category> AddAsync(Category entity);
    Task UpdateAsync(Category entity);
    Task DeleteAsync(Category entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<IReadOnlyList<Category>> GetPagedAsync(int page, int pageSize);
    Task<IReadOnlyList<Category>> FindAsync(Expression<Func<Category, bool>> predicate);

    // متدهای اختصاصی Category
    Task<Category?> GetCategoryWithSubCategoriesAsync(int id);
    Task<Category?> GetCategoryWithProductsAsync(int id);
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
    Task<List<Category>> GetMainCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> HasProductsAsync(int id);
    Task<bool> HasSubCategoriesAsync(int id);
}