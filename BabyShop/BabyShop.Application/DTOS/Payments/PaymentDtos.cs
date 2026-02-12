namespace BabyShop.Application.Dtos.Payments;

/// <summary>
/// ایجاد پرداخت
/// </summary>
public sealed record CreatePaymentRequest
{
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
}

/// <summary>
/// تایید پرداخت
/// </summary>
public sealed record VerifyPaymentRequest
{
    public string Authority { get; init; } = string.Empty;
}

/// <summary>
/// نتیجه تایید پرداخت
/// </summary>
public sealed record VerifyPaymentResponse
{
    public bool Success { get; init; }
    public int? OrderId { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// پاسخ پرداخت
/// </summary>
public sealed record PaymentResponse
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Authority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? TransactionId { get; init; }
    public DateTime? PaidAt { get; init; }
    public DateTime CreatedAt { get; init; }
}