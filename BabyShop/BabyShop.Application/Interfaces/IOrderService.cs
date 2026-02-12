using BabyShop.Services.BabyShop.Application.Dtos.Order;

namespace BabyShop.BabyShop.Application.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(int id);
    }
}