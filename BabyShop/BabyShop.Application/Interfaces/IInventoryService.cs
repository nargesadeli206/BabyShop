using BabyShop.Application.Dtos.Inventory;

namespace BabyShop.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryResponse> AddStockAsync(AddStockRequest request, CancellationToken cancellationToken = default);
    Task<InventoryResponse> RemoveStockAsync(RemoveStockRequest request, CancellationToken cancellationToken = default);
    Task<InventoryResponse?> GetInventoryByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryResponse>> GetAllInventoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryResponse>> GetLowStockInventoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> HasEnoughStockAsync(int productId, int quantity, CancellationToken cancellationToken);
}