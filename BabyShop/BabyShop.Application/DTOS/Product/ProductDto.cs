namespace BabyShop.Application.Dtos.Products;

/// <summary>
/// ایجاد محصول جدید
/// </summary>
public sealed record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
    public int InitialStock { get; init; }
}

/// <summary>
/// به‌روزرسانی محصول
/// </summary>
public sealed record UpdateProductRequest
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

/// <summary>
/// پاسخ محصول
/// </summary>
public sealed record ProductResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// خلاصه محصول برای لیست
/// </summary>
public sealed record ProductSummaryResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
}