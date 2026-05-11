using BabyShop.Core.Enums;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Basket
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public BasketStatus Status { get; set; } = BasketStatus.Active;
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public int DiscountPercentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();

    // Business Logic Properties
    public decimal SubTotal => Items?.Sum(x => x.TotalPrice) ?? 0;
    public decimal TotalDiscount => DiscountAmount;
    public decimal TotalPrice => SubTotal - DiscountAmount;
    public int TotalItems => Items?.Sum(x => x.Quantity) ?? 0;

    // Business Methods
    public void ApplyDiscount(string code, decimal amount, int percentage)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("کد تخفیف نمی‌تواند خالی باشد");

        if (amount < 0)
            throw new ArgumentException("مقدار تخفیف نمی‌تواند منفی باشد");

        if (amount > SubTotal)
            throw new BusinessRuleException("مبلغ تخفیف نمی‌تواند از کل سبد بیشتر باشد");

        DiscountCode = code;
        DiscountAmount = amount;
        DiscountPercentage = percentage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDiscount()
    {
        DiscountCode = null;
        DiscountAmount = 0;
        DiscountPercentage = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Product product, int quantity)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentException("تعداد باید بیشتر از صفر باشد");

        if (product.StockQuantity < quantity)
            throw new BusinessRuleException($"موجودی محصول {product.Name} کافی نیست");

        var existingItem = Items.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new BasketItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                UnitPrice = product.Price,
                CreatedAt = DateTime.UtcNow
            });
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateItemQuantity(int productId, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new NotFoundException("محصول در سبد خرید یافت نشد");

        if (quantity <= 0)
        {
            Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();
        RemoveDiscount();
        UpdatedAt = DateTime.UtcNow;
    }
}