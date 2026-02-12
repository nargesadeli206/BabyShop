namespace BabyShop.Core.Enums;

/// <summary>
/// وضعیت پرداخت
/// </summary>
public enum PaymentStatus
{
    Pending = 0,    // در انتظار
    Paid = 1,       // پرداخت شده
    Failed = 2,     // ناموفق
    Refunded = 3    // برگشت داده شده
}