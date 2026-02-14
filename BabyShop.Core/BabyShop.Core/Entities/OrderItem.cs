using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    public Order? Order { get; set; }

    private OrderItem() { }

    public OrderItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("Amount must be greater than zero.");
        Quantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new BusinessRuleException("Amount must be greater than zero.");
        if (Quantity - amount <= 0)
            throw new BusinessRuleException("Quantity cannot be zero or negative.");

        Quantity -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}