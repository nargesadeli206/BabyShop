using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// دسته‌بندی محصولات
/// </summary>
public sealed class Category : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int? ParentCategoryId { get; private set; }

    // ارتباطات
    public Category ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; }
    public ICollection<Product> Products { get; private set; }

    private Category() { } // برای EF Core

    public Category(string name, string? description = null, int? parentCategoryId = null)
    {
        SetName(name);
        Description = description?.Trim() ?? string.Empty;
        ParentCategoryId = parentCategoryId;

        SubCategories = new HashSet<Category>();
        Products = new HashSet<Product>();
    }

    /// <summary>
    /// به‌روزرسانی دسته‌بندی
    /// </summary>
    public void Update(string name, string? description = null)
    {
        SetName(name);
        Description = description?.Trim() ?? string.Empty;
        UpdateAuditFields();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");

        Name = name.Trim();
    }

    /// <summary>
    /// اضافه کردن زیردسته
    /// </summary>
    public void AddSubCategory(Category subCategory)
    {
        ArgumentNullException.ThrowIfNull(subCategory);

        SubCategories.Add(subCategory);
        UpdateAuditFields();
    }
}