using BabyShop.Core.Entities;
using BabyShop.Core.Enums;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// موجودیت ارسال
/// </summary>
public sealed class Delivery : BaseEntity
{
    public int OrderId { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public string PostalCode { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }

    // ارتباطات
    public Order Order { get; private set; }

    private Delivery() { } // برای EF Core

    public Delivery(int orderId, string address, string phoneNumber, string postalCode)
    {
        OrderId = orderId;
        SetAddress(address);
        SetPhoneNumber(phoneNumber);
        SetPostalCode(postalCode);

        Status = DeliveryStatus.Pending;
    }

    /// <summary>
    /// تنظیم تاریخ تخمینی تحویل
    /// </summary>
    public void SetEstimatedDeliveryDate(DateTime estimatedDate)
    {
        if (estimatedDate < DateTime.UtcNow.Date)
            throw new DomainException("Estimated delivery date cannot be in the past.");

        EstimatedDeliveryDate = estimatedDate;
        UpdateAuditFields();
    }

    /// <summary>
    /// در حال پردازش
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = DeliveryStatus.Processing;
        UpdateAuditFields();
    }

    /// <summary>
    /// ارسال شده
    /// </summary>
    public void MarkAsShipped(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException("Tracking number cannot be empty.");

        Status = DeliveryStatus.Shipped;
        TrackingNumber = trackingNumber;
        UpdateAuditFields();
    }

    /// <summary>
    /// تحویل شده
    /// </summary>
    public void MarkAsDelivered()
    {
        Status = DeliveryStatus.Delivered;
        ActualDeliveryDate = DateTime.UtcNow;
        UpdateAuditFields();
    }

    /// <summary>
    /// ارسال ناموفق
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        Status = DeliveryStatus.Failed;
        UpdateAuditFields();
    }

    private void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Address cannot be empty.");

        Address = address.Trim();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number cannot be empty.");

        PhoneNumber = phoneNumber.Trim();
    }

    private void SetPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Postal code cannot be empty.");

        PostalCode = postalCode.Trim();
    }
}