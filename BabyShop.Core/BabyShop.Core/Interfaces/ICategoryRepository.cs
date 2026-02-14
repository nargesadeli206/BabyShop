using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetCategoryWithSubCategoriesAsync(int id);
    Task<IReadOnlyList<Category>> GetMainCategoriesAsync();
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync();
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
}