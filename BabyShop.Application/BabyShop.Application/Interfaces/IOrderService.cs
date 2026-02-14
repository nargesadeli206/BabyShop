using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<PagedResultDto<OrderSummaryDto>> GetOrdersPagedAsync(int pageNumber, int pageSize);
    Task<List<OrderSummaryDto>> GetOrdersByUserAsync(int userId);
    Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
    Task CancelOrderAsync(int orderId, string reason);
}