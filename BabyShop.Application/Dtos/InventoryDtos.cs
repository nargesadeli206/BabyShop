using System;

namespace BabyShop.Application.Dtos;

public class CreateInventoryDto
{
    public int ProductId { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; } = 0;
}

public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateInventoryDto
{
    public int ProductId { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public int Quantity { get; set; }
}