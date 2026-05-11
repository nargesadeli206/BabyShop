using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;
using BabyShop.Core.ValueObjects;

namespace BabyShop.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public Gender Gender { get; private set; } = null!;
    public AgeRange AgeRange { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public int ViewCount { get; private set; }
    public int SoldCount { get; private set; }
    public string? MainImage { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual Inventory? Inventory { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();

    private Product() { }

    public Product(
        string name,
        string description,
        decimal price,
        int categoryId,
        Gender gender,
        AgeRange ageRange)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        SetPrice(price);
        CategoryId = categoryId;
        SetGender(gender);
        SetAgeRange(ageRange);
        IsActive = true;
        ViewCount = 0;
        SoldCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        Gender gender,
        AgeRange ageRange)
    {
        SetName(name);
        SetSlug(name);
        SetDescription(description);
        SetPrice(price);
        SetGender(gender);
        SetAgeRange(ageRange);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void IncrementViewCount() => ViewCount++;
    public void IncrementSoldCount(int quantity) => SoldCount += quantity;
    public void SetMainImage(string imageUrl) => MainImage = imageUrl;

    public int StockQuantity => Inventory?.CurrentStock ?? 0;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("نام محصول نمی‌تواند خالی باشد.");
        if (name.Length < 3)
            throw new BusinessRuleException("نام محصول باید حداقل ۳ کاراکتر باشد.");
        Name = name.Trim();
    }

    private void SetSlug(string name)
    {
        Slug = name.Trim()
            .ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("،", "")
            .Replace(",", "");
    }

    private void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new BusinessRuleException("قیمت محصول باید بیشتر از صفر باشد.");
        Price = price;
    }

    private void SetGender(Gender gender)
    {
        Gender = gender ?? throw new BusinessRuleException("جنسیت نمی‌تواند خالی باشد.");
    }

    private void SetAgeRange(AgeRange ageRange)
    {
        AgeRange = ageRange ?? throw new BusinessRuleException("رده سنی نمی‌تواند خالی باشد.");
    }
}