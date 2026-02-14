using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int ViewCount { get; private set; }
    public int SoldCount { get; private set; }

    public Category? Category { get; set; }
    public Inventory? Inventory { get; set; }

    private Product() { }

    public Product(string name, string description, decimal price, int categoryId)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        SetPrice(price);
        CategoryId = categoryId;
        IsActive = true;
        ViewCount = 0;
        SoldCount = 0;
    }

    public void Update(string name, string description, decimal price)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        SetPrice(price);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void IncrementViewCount() => ViewCount++;
    public void IncrementSoldCount(int quantity) => SoldCount += quantity;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");
        if (name.Length < 3)
            throw new DomainException("Product name must be at least 3 characters.");
        Name = name.Trim();
    }

    private void SetSlug(string name)
    {
        Slug = name.Trim().ToLower().Replace(" ", "-").Replace("'", "").Replace("\"", "");
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Product description cannot be empty.");
        Description = description.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Product price must be greater than zero.");
        Price = price;
    }
}