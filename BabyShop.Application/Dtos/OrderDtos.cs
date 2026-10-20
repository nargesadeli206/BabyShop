using System;
using System.Collections.Generic;

namespace BabyShop.Application.Dtos;

/// <summary>
/// مدل مخصوص ایجاد سفارش جدید
/// </summary>
public class CreateOrderDto
{
    public int UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CustomerNote { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// مدل آیتم‌های سفارش در زمان ثبت
/// </summary>
public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// مدل ثبت سفارش مستقیم از سبد خرید
/// </summary>
public class CreateOrderFromBasketDto
{
    public int UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int BasketId { get; set; }
}

/// <summary>
/// مدل تغییر وضعیت سفارش توسط ادمین
/// </summary>
public class UpdateOrderStatusDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// مدل خلاصه سفارش برای لیست اصلی (مخصوص پنل ادمین)
/// </summary>
public class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }

    // --- فیلدهای بیزنسی برای ادمین ---
    public string UserName { get; set; } = string.Empty; // نام واقعی مشتری از دیتابیس
    public string PhoneNumber { get; set; } = string.Empty; // شماره تماس ثبت شده در سفارش
}

/// <summary>
/// مدل کامل سفارش همراه با جزئیات اقلام (برای فاکتور)
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CustomerNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// مدل جزئیات هر کالا در فاکتور نهایی
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}