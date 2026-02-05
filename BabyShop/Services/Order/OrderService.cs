public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<int> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("Cart is empty");

        var order = new Order
        {
            UserId = dto.UserId,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.Pending,
            OrderItems = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            decimal price = 100;

            order.OrderItems.Add(new sOrderItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = price
            });

            total += price * item.Quantity;
        }

        order.TotalPrice = total;

        await _orderRepository.AddAsync(order);

        return order.Id;
    }
}

internal class sOrderItem : OrderItem
{
    public object ProductVariantId { get; set; }
    public object Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

internal class OrderItem
{
}