namespace BabyShop.Core.Entities;

public class BasketItem
{
    public int Id { get; set; }
    public int BasketId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Basket? Basket { get; set; }
    public virtual Product? Product { get; set; }

    // Business Logic Properties
    public decimal TotalPrice => Quantity * UnitPrice;
}