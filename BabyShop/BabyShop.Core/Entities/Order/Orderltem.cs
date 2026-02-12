using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// آیتم سفارش
/// </summary>
public sealed class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    // ارتباطات
    public Order Order { get; private set; }
    public Product Product { get; private set; }

    private OrderItem() { } // برای EF Core

    public OrderItem(int productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// افزایش تعداد
    /// </summary>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        Quantity += amount;
        UpdateAuditFields();
    }

    /// <summary>
    /// کاهش تعداد
    /// </summary>
    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        if (Quantity - amount <= 0)
            throw new DomainException("Quantity cannot be zero or negative.");

        Quantity -= amount;
        UpdateAuditFields();
    }
}