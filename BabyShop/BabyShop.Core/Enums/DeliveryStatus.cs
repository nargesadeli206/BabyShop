namespace BabyShop.Core.Enums;

/// <summary>
/// وضعیت ارسال
/// </summary>
public enum DeliveryStatus
{
    Pending = 0,     // در انتظار
    Processing = 1,  // در حال پردازش
    Shipped = 2,     // ارسال شده
    Delivered = 3,   // تحویل شده
    Failed = 4       // ناموفق
}