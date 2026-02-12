namespace BabyShop.Application.Dtos.Deliveries;

/// <summary>
/// ایجاد ارسال
/// </summary>
public sealed record CreateDeliveryRequest
{
    public int OrderId { get; init; }
    public string Address { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
}

/// <summary>
/// به‌روزرسانی وضعیت ارسال
/// </summary>
public sealed record UpdateDeliveryStatusRequest
{
    public int DeliveryId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
}

/// <summary>
/// پاسخ ارسال
/// </summary>
public sealed record DeliveryResponse
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public string Address { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? EstimatedDeliveryDate { get; init; }
    public DateTime? ActualDeliveryDate { get; init; }
    public string? TrackingNumber { get; init; }
    public DateTime CreatedAt { get; init; }
}