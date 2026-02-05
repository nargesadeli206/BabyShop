using BabyShop.Dtos;
using BabyShop.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory?> GetByIdAsync(int id);
        Task<Inventory?> GetByProductIdAsync(int productId);
        Task AddAsync(Inventory inventory);
        Task UpdateAsync(UpdateInventoryDto dto);
        Task DeleteAsync(int id);
    }
}