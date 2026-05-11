using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class Basket : BaseEntity
{
    public int UserId { get; set; }
    public string Status { get; set; } = "Active";
    public decimal TotalPrice { get; private set; }
    public string? DiscountCode { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation properties
    public virtual User? User { get; set; }
    public virtual ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();

    // Business properties
    public decimal SubTotal => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0;
    public decimal TotalDiscount => DiscountAmount ?? 0;
    public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;

    // Business methods
    public bool IsExpired() => ExpiredAt.HasValue && ExpiredAt.Value < DateTime.UtcNow;
    public bool CanAddItem() => Status == "Active" && !IsExpired();

    public void SetTotalPrice(decimal totalPrice)
    {
        TotalPrice = totalPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(string code, decimal amount, decimal? percentage = null)
    {
        DiscountCode = code;
        DiscountAmount = amount;
        DiscountPercentage = percentage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDiscount()
    {
        DiscountCode = null;
        DiscountAmount = null;
        DiscountPercentage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConvertToOrder()
    {
        Status = "ConvertedToOrder";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        Status = "Abandoned";
        UpdatedAt = DateTime.UtcNow;
    }
}