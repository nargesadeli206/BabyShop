using System;

namespace BabyShop.Application.Dtos;

public class CreateInventoryDto
{
    public int ProductId { get; set; }
    public int Stock { get; set; }
    public int? MinimumStockLevel { get; set; }
    public int? MaximumStockLevel { get; set; }
    public int? ReorderPoint { get; set; }
    public string? Location { get; set; }
}

public class UpdateInventoryDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class InventoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public int MaximumStockLevel { get; set; }
    public int ReorderPoint { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool NeedsReorder { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}