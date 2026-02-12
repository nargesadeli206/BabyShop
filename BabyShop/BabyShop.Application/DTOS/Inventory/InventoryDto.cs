namespace BabyShop.Application.Dtos.Inventory;

/// <summary>
/// درخواست اضافه کردن موجودی
/// </summary>
public sealed record AddStockRequest
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public string Reason { get; init; } = "Manual adjustment";
}

/// <summary>
/// درخواست کم کردن موجودی
/// </summary>
public sealed record RemoveStockRequest
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public string Reason { get; init; } = "Manual adjustment";
}

/// <summary>
/// پاسخ موجودی
/// </summary>
public sealed record InventoryResponse
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int MinimumStockLevel { get; init; }
    public int MaximumStockLevel { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
}