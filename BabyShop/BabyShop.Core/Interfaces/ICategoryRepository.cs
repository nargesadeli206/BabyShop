using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    // Check slug
    Task<bool> IsSlugUniqueAsync(string slug, int? id = null);

    // Get with subcategories
    Task<Category?> GetCategoryWithSubCategoriesAsync(int id);

    // Get with products
    Task<Category?> GetCategoryWithProductsAsync(int id);

    // Get main categories (ParentCategoryId == null)
    Task<List<Category>> GetMainCategoriesAsync();

    // Get subcategories
    Task<List<Category>> GetSubCategoriesAsync(int parentId);

    // Check methods
    Task<bool> HasProductsAsync(int id);
    Task<bool> HasSubCategoriesAsync(int id);

    // For paging
    Task<IQueryable<Category>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Category> query);
    Task<List<Category>> GetPagedAsync(IQueryable<Category> query, int pageNumber, int pageSize);
}