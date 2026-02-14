using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
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

        var existing = await _inventoryRepository.GetByProductIdAsync(dto.ProductId);
        if (existing != null)
            throw new BusinessRuleException($"Inventory already exists for product {dto.ProductId}");

        var inventory = new Inventory(dto.ProductId, dto.Stock);

        if (dto.MinimumStockLevel.HasValue)
            inventory.UpdateThresholds(
                dto.MinimumStockLevel.Value,
                dto.MaximumStockLevel ?? 1000,
                dto.ReorderPoint ?? 10);

        await _inventoryRepository.AddAsync(inventory);
        _logger.LogInformation("Inventory created for product {ProductId}", dto.ProductId);

        return await MapToInventoryDto(inventory);
    }

    public async Task<InventoryDto> IncreaseStockAsync(UpdateInventoryDto dto)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(dto.ProductId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {dto.ProductId} not found");

        inventory.AddStock(dto.Quantity);
        await _inventoryRepository.UpdateAsync(inventory);

        _logger.LogInformation("Stock increased for product {ProductId}: +{Quantity}", dto.ProductId, dto.Quantity);
        return await MapToInventoryDto(inventory);
    }

    public async Task<InventoryDto> DecreaseStockAsync(UpdateInventoryDto dto)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(dto.ProductId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {dto.ProductId} not found");

        inventory.RemoveStock(dto.Quantity);
        await _inventoryRepository.UpdateAsync(inventory);

        _logger.LogInformation("Stock decreased for product {ProductId}: -{Quantity}", dto.ProductId, dto.Quantity);
        return await MapToInventoryDto(inventory);
    }

    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null || inventory.IsDeleted)
            return null;

        return await MapToInventoryDto(inventory);
    }

    public async Task<List<InventoryDto>> GetAllInventoriesAsync()
    {
        var inventories = await _inventoryRepository.GetAllAsync();
        var result = new List<InventoryDto>();

        foreach (var inventory in inventories.Where(i => !i.IsDeleted))
        {
            result.Add(await MapToInventoryDto(inventory));
        }

        return result;
    }

    public async Task<List<InventoryDto>> GetLowStockInventoriesAsync()
    {
        var inventories = await _inventoryRepository.GetLowStockAsync();
        var result = new List<InventoryDto>();

        foreach (var inventory in inventories)
        {
            result.Add(await MapToInventoryDto(inventory));
        }

        return result;
    }

    public async Task<bool> HasEnoughStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return inventory != null && inventory.CurrentStock >= quantity;
    }

    public async Task ReserveStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {productId} not found");

        inventory.ReserveStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);
    }

    public async Task ReleaseReservedStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            throw new NotFoundException($"Inventory for product {productId} not found");

        inventory.ReleaseReservedStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);
    }

    private async Task<InventoryDto> MapToInventoryDto(Inventory inventory)
    {
        var product = await _productRepository.GetByIdAsync(inventory.ProductId);

        return new InventoryDto
        {
            ProductId = inventory.ProductId,
            ProductName = product?.Name ?? "Unknown",
            CurrentStock = inventory.CurrentStock,
            ReservedStock = inventory.ReservedStock,
            AvailableStock = inventory.AvailableStock,
            MinimumStockLevel = inventory.MinimumStockLevel,
            MaximumStockLevel = inventory.MaximumStockLevel,
            ReorderPoint = inventory.ReorderPoint,
            IsLowStock = inventory.IsLowStock,
            IsOutOfStock = inventory.IsOutOfStock,
            NeedsReorder = inventory.NeedsReorder,
            Location = inventory.Location,
            LastUpdated = inventory.UpdatedAt ?? inventory.CreatedAt
        };
    }
}