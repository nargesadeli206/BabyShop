namespace BabyShop.Core.Enums;

/// <summary>
/// وضعیت سفارش
/// </summary>
public enum OrderStatus
{
    Pending = 0,      // در انتظار پرداخت
    Paid = 1,         // پرداخت شده
    Shipped = 2,      // ارسال شده
    Delivered = 3,    // تحویل شده
    Cancelled = 4     // لغو شده
}