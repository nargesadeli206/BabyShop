using BabyShop.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.Core.Interfaces;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<Inventory>> GetAllWithProductAsync();
    Task<IReadOnlyList<Inventory>> GetLowStockInventoriesAsync();

    Task IncreaseStockAsync(int productId, int quantity);
    Task DecreaseStockAsync(int productId, int quantity);

    // متدهای اختصاصی رده سنی (اینها به اینترفیس اضافه شدند)
    Task<List<Inventory>> GetAllInventoriesByProductIdAsync(int productId);
    Task<Inventory?> GetProductInventoryByAgeRangeAsync(int productId, string ageRange);
    Task IncreaseStockAsync(int productId, string ageRange, int quantity);
    Task DecreaseStockAsync(int productId, string ageRange, int quantity);
}