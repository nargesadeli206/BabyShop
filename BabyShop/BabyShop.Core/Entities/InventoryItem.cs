using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// موجودیت انبار - مدیریت موجودی
/// </summary>
public sealed class Inventory : BaseEntity
{
    public int ProductId { get; private set; }
    public int CurrentStock { get; private set; }
    public int MinimumStockLevel { get; private set; }
    public int MaximumStockLevel { get; private set; }
    public string Location { get; private set; }

    // ارتباطات
    public Product Product { get; private set; }

    private Inventory() { } // برای EF Core

    public Inventory(int productId, int initialStock, int minimumStock = 5, int maximumStock = 1000)
    {
        ProductId = productId;
        MinimumStockLevel = minimumStock;
        MaximumStockLevel = maximumStock;
        Location = "Main Warehouse";

        AddStock(initialStock, "Initial stock");
    }

    /// <summary>
    /// اضافه کردن موجودی
    /// </summary>
    public void AddStock(int quantity, string reason)
    {
        ValidateQuantity(quantity);

        if (CurrentStock + quantity > MaximumStockLevel)
            throw new DomainException($"Cannot add {quantity} units. Maximum capacity is {MaximumStockLevel}.");

        CurrentStock += quantity;
        UpdateAuditFields();
    }

    /// <summary>
    /// کم کردن موجودی
    /// </summary>
    public void RemoveStock(int quantity, string reason)
    {
        ValidateQuantity(quantity);

        if (CurrentStock - quantity < 0)
            throw new DomainException($"Insufficient stock. Available: {CurrentStock}, Requested: {quantity}.");

        CurrentStock -= quantity;
        UpdateAuditFields();
    }

    /// <summary>
    /// تغییر محل انبار
    /// </summary>
    public void UpdateLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new DomainException("Location cannot be empty.");

        Location = location.Trim();
        UpdateAuditFields();
    }

    /// <summary>
    /// آیا موجودی کم است؟
    /// </summary>
    public bool IsLowStock => CurrentStock < MinimumStockLevel;

    /// <summary>
    /// آیا موجودی تمام شده است؟
    /// </summary>
    public bool IsOutOfStock => CurrentStock == 0;

    private void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
    }
}