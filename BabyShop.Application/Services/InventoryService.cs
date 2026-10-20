using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyShop.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<InventoryDto> IncreaseStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
        {
            inventory = new Inventory(productId, "Unisex", quantity);
            await _inventoryRepository.AddAsync(inventory);
        }
        else
        {
            inventory.IncreaseStock(quantity);
            await _inventoryRepository.UpdateAsync(inventory);
        }
        return MapToDto(inventory);
    }

    public async Task<InventoryDto> DecreaseStockAsync(int productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId)
                        ?? throw new NotFoundException("محصول در انبار یافت نشد");

        inventory.DecreaseStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);
        return MapToDto(inventory);
    }

    public async Task<InventoryDto> IncreaseStockAsync(int productId, string ageRange, int quantity)
    {
        var existing = await _inventoryRepository.GetProductInventoryByAgeRangeAsync(productId, ageRange);
        if (existing == null)
        {
            existing = new Inventory(productId, ageRange, quantity);
            await _inventoryRepository.AddAsync(existing);
        }
        else
        {
            existing.IncreaseStock(quantity);
            await _inventoryRepository.UpdateAsync(existing);
        }
        return MapToDto(existing);
    }

    public async Task<InventoryDto> DecreaseStockAsync(int productId, string ageRange, int quantity)
    {
        var inventory = await _inventoryRepository.GetProductInventoryByAgeRangeAsync(productId, ageRange)
                        ?? throw new NotFoundException($"محصول با سایز {ageRange} در انبار یافت نشد");

        inventory.DecreaseStock(quantity);
        await _inventoryRepository.UpdateAsync(inventory);
        return MapToDto(inventory);
    }

    public async Task<List<InventoryDto>> GetAllInventoriesAsync()
    {
        var data = await _inventoryRepository.GetAllWithProductAsync();
        return data.Select(MapToDto).ToList();
    }

    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inv = await _inventoryRepository.GetByProductIdAsync(productId);
        return inv == null ? null : MapToDto(inv);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
    {
        var inv = await _inventoryRepository.GetByIdAsync(id);
        return inv == null ? null : MapToDto(inv);
    }

    public async Task DeleteInventoryAsync(int id)
    {
        var inv = await _inventoryRepository.GetByIdAsync(id);
        if (inv != null) await _inventoryRepository.DeleteAsync(inv);
    }

    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto)
    {
        var inv = new Inventory(dto.ProductId, dto.AgeRange, dto.CurrentStock);
        await _inventoryRepository.AddAsync(inv);
        return MapToDto(inv);
    }

    public async Task<List<InventoryDto>> GetLowStockInventoriesAsync()
    {
        var data = await _inventoryRepository.GetLowStockInventoriesAsync();
        return data.Select(MapToDto).ToList();
    }

    private InventoryDto MapToDto(Inventory inventory)
    {
        if (inventory == null) return new InventoryDto();
        return new InventoryDto
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            ProductName = inventory.Product?.Name ?? "محصول ثبت شده",
            AgeRange = inventory.AgeRange,
            CurrentStock = inventory.CurrentStock,
            ReservedStock = inventory.ReservedStock,
            AvailableStock = inventory.AvailableStock,
            MinimumStockLevel = inventory.MinimumStockLevel,
            IsLowStock = inventory.IsLowStock,
            IsOutOfStock = inventory.IsOutOfStock,
            CreatedAt = inventory.CreatedAt
        };
    }
}