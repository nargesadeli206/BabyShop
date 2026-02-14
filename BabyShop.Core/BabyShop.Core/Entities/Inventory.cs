using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; private set; }
    public int CurrentStock { get; private set; }
    public int ReservedStock { get; private set; }
    public int MinimumStockLevel { get; private set; } = 5;
    public int MaximumStockLevel { get; private set; } = 1000;
    public int ReorderPoint { get; private set; } = 10;
    public string Location { get; private set; } = "Main Warehouse";

    public Product? Product { get; set; }

    public int AvailableStock => CurrentStock - ReservedStock;
    public bool IsLowStock => CurrentStock < MinimumStockLevel;
    public bool IsOutOfStock => CurrentStock == 0;
    public bool NeedsReorder => CurrentStock <= ReorderPoint;

    private Inventory() { }

    public Inventory(int productId, int initialStock)
    {
        ProductId = productId;
        CurrentStock = initialStock;
        ReservedStock = 0;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        if (CurrentStock + quantity > MaximumStockLevel)
            throw new BusinessRuleException($"Cannot add {quantity} units. Maximum is {MaximumStockLevel}.");

        CurrentStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        if (CurrentStock - quantity < 0)
            throw new BusinessRuleException($"Insufficient stock. Available: {CurrentStock}");

        CurrentStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        if (AvailableStock < quantity)
            throw new BusinessRuleException($"Insufficient available stock. Available: {AvailableStock}");

        ReservedStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero.");
        if (ReservedStock < quantity)
            throw new BusinessRuleException($"Cannot release {quantity} units. Reserved: {ReservedStock}");

        ReservedStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateThresholds(int minimum, int maximum, int reorderPoint)
    {
        if (minimum < 0) throw new BusinessRuleException("Minimum stock cannot be negative.");
        if (maximum <= minimum) throw new BusinessRuleException("Maximum must be greater than minimum.");
        if (reorderPoint < minimum || reorderPoint > maximum)
            throw new BusinessRuleException("Reorder point must be between minimum and maximum.");

        MinimumStockLevel = minimum;
        MaximumStockLevel = maximum;
        ReorderPoint = reorderPoint;
        UpdatedAt = DateTime.UtcNow;
    }
}