using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces
{
    public interface IOrderService
    {
       
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetByIdAsync(int id);

        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<PagedResultDto<OrderSummaryDto>> GetOrdersPagedAsync(int pageNumber, int pageSize);
        Task<List<OrderSummaryDto>> GetOrdersByUserAsync(int userId);
        Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
        Task CancelOrderAsync(int orderId, string reason);
        Task<OrderDto> CreateOrderFromBasketAsync(int userId, CreateOrderFromBasketDto dto);
    }
}