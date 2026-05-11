using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Order : BaseEntity
{
    // ============ فیلدهای اصلی ============
    public int UserId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";

    // ============ فیلدهای مالی ============
    public decimal SubTotal { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // ============ یادداشت‌ها ============
    public string? CustomerNote { get; private set; }
    public string? AdminNote { get; private set; }

    // ============ زمان‌بندی ============
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // ============ وضعیت‌های اضافی ============
    public string? CancellationReason { get; private set; }
    public bool IsReminderSent { get; private set; }
    public DateTime? ReminderSentAt { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    // ============ Navigation Properties ============
    public virtual User? User { get; set; }
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public virtual Payment? Payment { get; set; }
    public virtual Delivery? Delivery { get; set; }

    // ============ پراپرتی‌های فقط خواندنی ============
    public bool IsPending => Status == "Pending";
    public bool IsPaid => Status == "Paid";
    public bool IsShipped => Status == "Shipped";
    public bool IsDelivered => Status == "Delivered";
    public bool IsCancelled => Status == "Cancelled";
    public bool IsRefunded => Status == "Refunded";
    public bool CanBeCancelled => Status == "Pending" || Status == "Paid";
    public int ItemsCount => Items.Count;

    // ============ سازنده‌ها ============
    private Order() { }

    public Order(int userId, string shippingAddress, string phoneNumber, string email = "")
    {
        if (userId <= 0)
            throw new BusinessRuleException("UserId is required");

        UserId = userId;
        SetShippingAddress(shippingAddress);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        OrderNumber = GenerateOrderNumber();
        Status = "Pending";
        SubTotal = 0;
        ShippingCost = 0;
        Tax = 0;
        Discount = 0;
        TotalAmount = 0;
        IsReminderSent = false;
        IsArchived = false;
        CreatedAt = DateTime.UtcNow;
    }

    // ============ متدهای کمکی خصوصی ============
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpper();
    }

    private void RecalculateTotal()
    {
        SubTotal = Items.Sum(i => i.TotalPrice);
        TotalAmount = SubTotal + ShippingCost + Tax - Discount;

        if (TotalAmount < 0)
            TotalAmount = 0;
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

    private void SetEmail(string email)
    {
        Email = email?.Trim() ?? string.Empty;
    }

    // ============ متدهای مدیریت آیتم‌ها ============

    public void AddItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Cannot add items to non-pending order.");
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        if (unitPrice <= 0)
            throw new BusinessRuleException("Unit price must be greater than zero.");

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

    // ============ متدهای اضافه برای Dapper (مهم) ============

    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new BusinessRuleException("Order item cannot be null.");

        item.OrderId = Id;
        Items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItems(IEnumerable<OrderItem> items)
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    // ============ بقیه متدهای مدیریت آیتم‌ها ============

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

    public void UpdateItemQuantity(int productId, int quantity)
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Cannot update items in non-pending order.");

        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new BusinessRuleException("Item not found in order.");

        if (quantity <= 0)
        {
            RemoveItem(productId);
        }
        else
        {
            item.SetQuantity(quantity);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ClearItems()
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Cannot clear items from non-pending order.");

        Items.Clear();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ متدهای تنظیم هزینه‌ها ============

    public void SetShippingCost(decimal cost)
    {
        if (cost < 0)
            throw new BusinessRuleException("Shipping cost cannot be negative.");

        ShippingCost = cost;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTax(decimal tax)
    {
        if (tax < 0)
            throw new BusinessRuleException("Tax cannot be negative.");

        Tax = tax;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDiscount(decimal discount)
    {
        if (discount < 0)
            throw new BusinessRuleException("Discount cannot be negative.");

        Discount = discount;
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ متدهای تغییر وضعیت ============

    public void MarkAsPaid()
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Order is not in pending status.");
        if (TotalAmount <= 0)
            throw new BusinessRuleException("Cannot pay for order with zero amount.");

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

    public void MarkAsRefunded()
    {
        if (Status != "Paid")
            throw new BusinessRuleException("Only paid orders can be refunded.");

        Status = "Refunded";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == "Delivered")
            throw new BusinessRuleException("Cannot cancel delivered order.");
        if (Status == "Shipped")
            throw new BusinessRuleException("Cannot cancel shipped order. Please contact support.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Cancellation reason is required.");

        Status = "Cancelled";
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ متدهای یادداشت ============

    public void AddCustomerNote(string note)
    {
        CustomerNote = note?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAdminNote(string note)
    {
        AdminNote = note?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ متدهای یادآوری ============

    public void MarkReminderSent()
    {
        IsReminderSent = true;
        ReminderSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ متدهای بایگانی ============

    public void Archive()
    {
        if (!IsArchived)
        {
            IsArchived = true;
            ArchivedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Unarchive()
    {
        if (IsArchived)
        {
            IsArchived = false;
            ArchivedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}