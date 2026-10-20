using BabyShop.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<InventoryDto> IncreaseStockAsync(int productId, int quantity);
    Task<InventoryDto> DecreaseStockAsync(int productId, int quantity);

    // متدهای جدید با سایز و نوع خروجی هماهنگ شده
    Task<InventoryDto> IncreaseStockAsync(int productId, string ageRange, int quantity);
    Task<InventoryDto> DecreaseStockAsync(int productId, string ageRange, int quantity);

    Task<List<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<InventoryDto?> GetInventoryByIdAsync(int id);
    Task DeleteInventoryAsync(int id);
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto dto);
    Task<List<InventoryDto>> GetLowStockInventoriesAsync();
}