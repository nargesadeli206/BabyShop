using BabyShop.Services.BabyShop.Application.Dtos.Deliveries;

namespace BabyShop.BabyShop.Application.Interfaces
{
    public interface IDeliveryService
    {
        Task<int> CreateAsync(CreateDeliveryDto dto);
        Task<DeliveryDto?> GetByIdAsync(int id);
    }
}