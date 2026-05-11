using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Category : Basket
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int? ParentCategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

    private Category() { }

    public Category(string name, string description, int? parentCategoryId = null,
                   string? imageUrl = null, int displayOrder = 0)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        ParentCategoryId = parentCategoryId;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    public void Update(string name, string description, string? imageUrl = null, int displayOrder = 0)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty.");
        Name = name.Trim();
    }

    private void SetSlug(string name)
    {
        Slug = name.Trim().ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("،", "");
    }

    private void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    public void AddSubCategory(Category subCategory)
    {
        if (subCategory == null)
            throw new DomainException("Subcategory cannot be null.");
        SubCategories.Add(subCategory);
        UpdatedAt = DateTime.UtcNow;
    }
}