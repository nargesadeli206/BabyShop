using BabyShop.Core.Entities;
using BabyShop.Core.Enums;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// موجودیت سفارش
/// </summary>
public sealed class Order : BaseEntity
{
    public int UserId { get; private set; }
    public string ShippingAddress { get; private set; }
    public string PhoneNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    // لیست آیتم‌های سفارش
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // ارتباطات
    public ICollection<Payment> Payments { get; private set; }
    public Delivery Delivery { get; private set; }

    private Order() { } // برای EF Core

    public Order(int userId, string shippingAddress, string phoneNumber)
    {
        UserId = userId;
        SetShippingAddress(shippingAddress);
        SetPhoneNumber(phoneNumber);

        Status = OrderStatus.Pending;
        Payments = new HashSet<Payment>();
    }

    /// <summary>
    /// اضافه کردن آیتم به سفارش
    /// </summary>
    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, quantity, unitPrice));
        }

        RecalculateTotal();
        UpdateAuditFields();
    }

    /// <summary>
    /// حذف آیتم از سفارش
    /// </summary>
    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            _items.Remove(item);
            RecalculateTotal();
            UpdateAuditFields();
        }
    }

    /// <summary>
    /// محاسبه مجدد مبلغ کل
    /// </summary>
    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }

    /// <summary>
    /// پرداخت شده
    /// </summary>
    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        UpdateAuditFields();
    }

    /// <summary>
    /// ارسال شده
    /// </summary>
    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException("Cannot ship unpaid order.");

        Status = OrderStatus.Shipped;
        UpdateAuditFields();
    }

    /// <summary>
    /// تحویل شده
    /// </summary>
    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Cannot deliver unshipped order.");

        Status = OrderStatus.Delivered;
        UpdateAuditFields();
    }

    /// <summary>
    /// لغو سفارش
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel shipped or delivered order.");

        Status = OrderStatus.Cancelled;
        UpdateAuditFields();
    }

    private void SetShippingAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Shipping address cannot be empty.");

        ShippingAddress = address.Trim();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty.");

        PhoneNumber = phoneNumber.Trim();
    }
}