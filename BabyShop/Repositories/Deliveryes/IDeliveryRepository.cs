using BabyShop.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.Interfaces
{
    public interface IDeliveryRepository
    {
        Task<List<Delivery>> GetAllAsync();
        Task<Delivery?> GetByIdAsync(int id);
        Task AddAsync(Delivery delivery);
        Task DeleteAsync(int id);
    }
}