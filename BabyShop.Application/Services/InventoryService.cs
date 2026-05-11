using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IProductRepository productRepository,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new NotFoundException(nameof(Product), dto.ProductId);

        var inventory = new Inventory(dto.ProductId, dto.CurrentStock, dto.ReservedStock);

        if (dto.MinimumStockLevel > 0)
            inventory.SetMinimumStockLevel(dto.MinimumStockLevel);
        if (dto.MaximumStockLevel > 0)
            inventory.SetMaximumStockLevel(dto.MaximumStockLevel);
        if (dto.ReorderPoint > 0)
            inventory.SetReorderPoint(dto.ReorderPoint);
        if (!string.IsNullOrEmpty(dto.Location))
            inventory.SetLocation(dto.Location);

        var created = await _inventoryRepository.AddAsync(inventory);
        _logger.LogInformation("Inventory created for product {ProductId}", dto.ProductId);

        return MapToDto(created);
    }

    public async Task<InventoryDto> IncreaseStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {productId} not found");

        inventory.IncreaseStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);

        _logger.LogInformation("Stock increased for product {ProductId} by {Quantity}", productId, quantity);
        return MapToDto(inventory);
    }

    public async Task<InventoryDto> DecreaseStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {productId} not found");

        inventory.DecreaseStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);

        _logger.LogInformation("Stock decreased for product {ProductId} by {Quantity}", productId, quantity);
        return MapToDto(inventory);
    }

    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return inventory == null ? null : MapToDto(inventory);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        return inventory == null ? null : MapToDto(inventory);
    }

    public async Task<List<InventoryDto>> GetAllInventoriesAsync()
    {
        var inventories = await _inventoryRepository.GetAllWithProductAsync();
        return inventories.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryDto>> GetLowStockInventoriesAsync()
    {
        var inventories = await _inventoryRepository.GetLowStockInventoriesAsync();
        return inventories.Select(MapToDto).ToList();
    }

    public async Task DeleteInventoryAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            throw new NotFoundException(nameof(Inventory), id);

        await _inventoryRepository.DeleteAsync(inventory);
        _logger.LogInformation("Inventory {Id} deleted", id);
    }

    private InventoryDto MapToDto(Inventory inventory)
    {
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.Product?.Name ?? string.Empty,
            CurrentStock = inventory.CurrentStock,
            ReservedStock = inventory.ReservedStock,
            AvailableStock = inventory.AvailableStock,
            MinimumStockLevel = inventory.MinimumStockLevel,
            MaximumStockLevel = inventory.MaximumStockLevel,
            ReorderPoint = inventory.ReorderPoint,
            Location = inventory.Location,
            IsLowStock = inventory.IsLowStock,
            IsOutOfStock = inventory.IsOutOfStock,
            CreatedAt = inventory.CreatedAt,
            UpdatedAt = inventory.UpdatedAt
        };
    }
}