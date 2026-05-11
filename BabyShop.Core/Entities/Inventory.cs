using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; private set; }
    public int CurrentStock { get; private set; }
    public int ReservedStock { get; private set; }
    public int MinimumStockLevel { get; private set; }
    public int MaximumStockLevel { get; private set; }
    public int ReorderPoint { get; private set; }
    public string Location { get; private set; } = string.Empty;

    // Navigation property
    public virtual Product? Product { get; set; }

    // Business properties
    public int AvailableStock => CurrentStock - ReservedStock;
    public bool IsLowStock => CurrentStock <= ReorderPoint;
    public bool IsOutOfStock => AvailableStock <= 0;

    private Inventory() { }

    public Inventory(int productId, int currentStock, int reservedStock = 0)
    {
        if (productId <= 0)
            throw new BusinessRuleException("ProductId is required");
        if (currentStock < 0)
            throw new BusinessRuleException("Current stock cannot be negative");
        if (reservedStock < 0)
            throw new BusinessRuleException("Reserved stock cannot be negative");

        ProductId = productId;
        CurrentStock = currentStock;
        ReservedStock = reservedStock;
        MinimumStockLevel = 5;
        MaximumStockLevel = 1000;
        ReorderPoint = 10;
        Location = "Main Warehouse";
        CreatedAt = DateTime.UtcNow;
    }

    public void SetMinimumStockLevel(int level)
    {
        if (level < 0)
            throw new BusinessRuleException("Minimum stock level cannot be negative");
        MinimumStockLevel = level;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaximumStockLevel(int level)
    {
        if (level < 0)
            throw new BusinessRuleException("Maximum stock level cannot be negative");
        if (level < MinimumStockLevel)
            throw new BusinessRuleException("Maximum stock level cannot be less than minimum stock level");
        MaximumStockLevel = level;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetReorderPoint(int reorderPoint)
    {
        if (reorderPoint < 0)
            throw new BusinessRuleException("Reorder point cannot be negative");
        ReorderPoint = reorderPoint;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new BusinessRuleException("Location cannot be empty");
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero");

        var newStock = CurrentStock + quantity;
        if (newStock > MaximumStockLevel)
            throw new BusinessRuleException($"Cannot exceed maximum stock level of {MaximumStockLevel}");

        CurrentStock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero");

        var newStock = CurrentStock - quantity;
        if (newStock < 0)
            throw new BusinessRuleException("Insufficient stock");

        CurrentStock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero");

        var available = AvailableStock;
        if (quantity > available)
            throw new BusinessRuleException($"Cannot reserve {quantity}. Only {available} available");

        ReservedStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero");
        if (quantity > ReservedStock)
            throw new BusinessRuleException($"Cannot release {quantity}. Only {ReservedStock} reserved");

        ReservedStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("Quantity must be greater than zero");
        if (quantity > ReservedStock)
            throw new BusinessRuleException($"Cannot confirm {quantity}. Only {ReservedStock} reserved");

        ReservedStock -= quantity;
        CurrentStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}