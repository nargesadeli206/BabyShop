using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto);
    Task<InventoryDto> IncreaseStockAsync(int productId, int quantity);
    Task<InventoryDto> DecreaseStockAsync(int productId, int quantity);
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<InventoryDto?> GetInventoryByIdAsync(int id);
    Task<List<InventoryDto>> GetAllInventoriesAsync();
    Task<List<InventoryDto>> GetLowStockInventoriesAsync();
    Task DeleteInventoryAsync(int id);
}