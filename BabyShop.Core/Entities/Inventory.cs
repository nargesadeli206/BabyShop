using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;
using System;

namespace BabyShop.Core.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; private set; }
    public string AgeRange { get; private set; } = string.Empty; // رده سنی (مثل 0-3)
    public int CurrentStock { get; private set; }
    public int ReservedStock { get; private set; }
    public int MinimumStockLevel { get; private set; }
    public int MaximumStockLevel { get; private set; }
    public int ReorderPoint { get; private set; }
    public string Location { get; private set; } = "Main Warehouse";

    public virtual Product? Product { get; set; }

    public int AvailableStock => CurrentStock - ReservedStock;
    public bool IsLowStock => CurrentStock <= ReorderPoint;
    public bool IsOutOfStock => AvailableStock <= 0;

    private Inventory() { }

    public Inventory(int productId, string ageRange, int currentStock)
    {
        ProductId = productId;
        AgeRange = ageRange;
        CurrentStock = currentStock;
        MinimumStockLevel = 5;
        MaximumStockLevel = 1000;
        ReorderPoint = 10;
        CreatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity) => CurrentStock += quantity;
    public void DecreaseStock(int quantity)
    {
        if (CurrentStock < quantity) throw new BusinessRuleException("موجودی کافی نیست");
        CurrentStock -= quantity;
    }
}