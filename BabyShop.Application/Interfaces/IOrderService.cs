using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId);
    Task<List<OrderSummaryDto>> GetUserOrderSummariesAsync(int userId);
    Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
    Task<OrderDto> CreateOrderFromBasketAsync(CreateOrderFromBasketDto dto);
    Task<OrderDto> CancelOrderAsync(int orderId, string reason);
}