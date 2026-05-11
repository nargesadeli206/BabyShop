// Core/Entities/Product.cs
using BabyShop.Core.Exceptions;
using BabyShop.Core.ValueObjects;

namespace BabyShop.Core.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }

    // موجودی از Inventory میاد
    public int StockQuantity { get; set; }

    public Gender Gender { get; private set; }
    public AgeRange AgeRange { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int ViewCount { get; private set; }
    public int SoldCount { get; private set; }

    // اضافه شد - MainImage برای تصویر محصول
    public string? MainImage { get; set; }

    // اضافه شد - ImageUrl به عنوان تصویر اصلی
    public string? ImageUrl { get; set; }

    // فیلدهای BaseEntity (برای Soft Delete)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public Inventory? Inventory { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<BasketItem> BasketItems { get; set; } = new List<BasketItem>();

    // برای EF Core
    private Product() { }

    // کانستراکتور اصلی
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

    // کانستراکتور با MainImage
    public Product(
        string name,
        string description,
        decimal price,
        int categoryId,
        Gender gender,
        AgeRange ageRange,
        string mainImage)
        : this(name, description, price, categoryId, gender, ageRange)
    {
        MainImage = mainImage;
        ImageUrl = mainImage;
    }

    // متد آپدیت
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

    // متد آپدیت با تصویر
    public void Update(
        string name,
        string description,
        decimal price,
        Gender gender,
        AgeRange ageRange,
        string mainImage)
    {
        Update(name, description, price, gender, ageRange);
        MainImage = mainImage;
        ImageUrl = mainImage;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void IncrementViewCount() => ViewCount++;
    public void IncrementSoldCount(int quantity) => SoldCount += quantity;

    // متد برای تنظیم تصویر
    public void SetMainImage(string imageUrl)
    {
        MainImage = imageUrl;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("نام محصول نمی‌تواند خالی باشد.");
        if (name.Length < 3)
            throw new DomainException("نام محصول باید حداقل ۳ کاراکتر باشد.");
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
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("توضیحات محصول نمی‌تواند خالی باشد.");
        Description = description.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("قیمت محصول باید بیشتر از صفر باشد.");
        Price = price;
    }

    private void SetGender(Gender gender)
    {
        Gender = gender ?? throw new DomainException("جنسیت نمی‌تواند خالی باشد.");
    }

    private void SetAgeRange(AgeRange ageRange)
    {
        AgeRange = ageRange ?? throw new DomainException("رده سنی نمی‌تواند خالی باشد.");
    }
}