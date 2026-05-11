using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto);
    Task<InventoryDto> IncreaseStockAsync(UpdateInventoryDto dto);
    Task<InventoryDto> DecreaseStockAsync(UpdateInventoryDto dto);
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<List<InventoryDto>> GetAllInventoriesAsync();
    Task<List<InventoryDto>> GetLowStockInventoriesAsync();
    Task<bool> HasEnoughStockAsync(int productId, int quantity);
    Task ReserveStockAsync(int productId, int quantity);
    Task ReleaseReservedStockAsync(int productId, int quantity);
}