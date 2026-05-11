using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Order : Basket
{

    public User? User { get; set; }

    public int UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public decimal SubTotal { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? CustomerNote { get; private set; }
    public string? AdminNote { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public bool IsReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public bool IsPaid { get; set; }


    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Delivery? Delivery { get; set; }

    private Order() { }

    public Order(int userId, string shippingAddress, string phoneNumber, string email = "")
    {
        UserId = userId;
        SetShippingAddress(shippingAddress);
        SetPhoneNumber(phoneNumber);
        Email = email;
        OrderNumber = GenerateOrderNumber();
        Status = "Pending";
        SubTotal = 0;
        ShippingCost = 0;
        Tax = 0;
        Discount = 0;
        TotalAmount = 0;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpper();
    }

    public void AddItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Cannot add items to non-pending order.");
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            Items.Add(new OrderItem(productId, productName, quantity, unitPrice));
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(int productId)
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Cannot remove items from non-pending order.");

        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private void RecalculateTotal()
    {
        SubTotal = Items.Sum(i => i.TotalPrice);
        TotalAmount = SubTotal + ShippingCost + Tax - Discount;
    }

    public void ApplyDiscount(decimal discount)
    {
        if (discount < 0)
            throw new BusinessRuleException("Discount cannot be negative.");
        Discount = discount;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Order is not in pending status.");

        Status = "Paid";
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsShipped()
    {
        if (Status != "Paid")
            throw new BusinessRuleException("Cannot ship unpaid order.");

        Status = "Shipped";
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != "Shipped")
            throw new BusinessRuleException("Cannot deliver unshipped order.");

        Status = "Delivered";
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == "Delivered" || Status == "Shipped")
            throw new BusinessRuleException("Cannot cancel shipped or delivered order.");

        Status = "Cancelled";
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetShippingAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessRuleException("Shipping address cannot be empty.");
        ShippingAddress = address.Trim();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessRuleException("Phone number cannot be empty.");
        PhoneNumber = phoneNumber.Trim();
    }
}