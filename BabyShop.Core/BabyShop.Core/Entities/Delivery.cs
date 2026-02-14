using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Delivery : BaseEntity
{
    public int OrderId { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? Carrier { get; private set; }

    public Order? Order { get; set; }

    private Delivery() { }

    public Delivery(int orderId, string address, string phoneNumber, string postalCode, string? carrier = null)
    {
        OrderId = orderId;
        SetAddress(address);
        SetPhoneNumber(phoneNumber);
        SetPostalCode(postalCode);
        Status = "Pending";
        Carrier = carrier ?? "Standard";
        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3);
    }

    public void SetEstimatedDeliveryDate(DateTime estimatedDate)
    {
        if (estimatedDate < DateTime.UtcNow.Date)
            throw new BusinessRuleException("Estimated delivery date cannot be in the past.");
        EstimatedDeliveryDate = estimatedDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = "Processing";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsShipped(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new BusinessRuleException("Tracking number cannot be empty.");

        Status = "Shipped";
        TrackingNumber = trackingNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = "Delivered";
        ActualDeliveryDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = "Failed";
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new BusinessRuleException("Address cannot be empty.");
        Address = address.Trim();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new BusinessRuleException("Phone number cannot be empty.");
        PhoneNumber = phoneNumber.Trim();
    }

    private void SetPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new BusinessRuleException("Postal code cannot be empty.");
        PostalCode = postalCode.Trim();
    }
}