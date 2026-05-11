using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class BasketItem : BaseEntity
{
    public int BasketId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPerUnit { get; set; }

    // Navigation properties
    public virtual Basket Basket { get; set; }
    public virtual Product Product { get; set; }

    // Business properties
    public decimal TotalPrice => (UnitPrice - (DiscountPerUnit ?? 0)) * Quantity;

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        Quantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        if (Quantity - amount < 0) throw new InvalidOperationException("Quantity cannot be negative");
        Quantity -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentException("Quantity cannot be negative");
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}