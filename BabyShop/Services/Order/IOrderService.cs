public interface IOrderService
{
    Task<int> CreateOrderAsync(CreateOrderDto dto);
}