using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// موجودیت محصول - هسته اصلی کسب و کار
/// </summary>
public sealed class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsActive { get; private set; }

    // ارتباطات
    public Category Category { get; private set; }
    public Inventory Inventory { get; private set; }

    private Product() { } // برای EF Core

    public Product(string name, string description, decimal price, int categoryId)
    {
        SetName(name);
        SetDescription(description);
        SetPrice(price);

        CategoryId = categoryId;
        IsActive = true;
    }

    /// <summary>
    /// به‌روزرسانی اطلاعات محصول
    /// </summary>
    public void Update(string name, string description, decimal price)
    {
        SetName(name);
        SetDescription(description);
        SetPrice(price);

        UpdateAuditFields();
    }

    /// <summary>
    /// فعال کردن محصول
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateAuditFields();
    }

    /// <summary>
    /// غیرفعال کردن محصول
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdateAuditFields();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");

        if (name.Length < 3)
            throw new DomainException("Product name must be at least 3 characters.");

        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Product price must be greater than zero.");

        if (price > 100_000_000)
            throw new DomainException("Product price cannot exceed 100,000,000.");

        Price = price;
    }
}