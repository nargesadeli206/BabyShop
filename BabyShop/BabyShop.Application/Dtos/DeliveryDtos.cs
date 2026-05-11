using System;

namespace BabyShop.Application.Dtos;

public class CreateDeliveryDto
{
    public int OrderId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string? Carrier { get; set; }
}

public class UpdateDeliveryStatusDto
{
    public int DeliveryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
}

public class DeliveryDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public DateTime CreatedAt { get; set; }
}