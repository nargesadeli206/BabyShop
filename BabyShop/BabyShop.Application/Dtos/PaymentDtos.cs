using System;

namespace BabyShop.Application.Dtos;

public class CreatePaymentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class VerifyPaymentDto
{
    public string Authority { get; set; } = string.Empty;
}

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}