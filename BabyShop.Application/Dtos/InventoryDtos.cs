namespace BabyShop.Application.Dtos;

public class CreateInventoryDto
{
    public int ProductId { get; set; }
    public int CurrentStock { get; set; } = 0;
    public int ReservedStock { get; set; } = 0;
    public int MinimumStockLevel { get; set; } = 5;
    public int MaximumStockLevel { get; set; } = 1000;
    public int ReorderPoint { get; set; } = 10;
    public string Location { get; set; } = "Main Warehouse";
}

public class UpdateInventoryDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public int MaximumStockLevel { get; set; }
    public int ReorderPoint { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}