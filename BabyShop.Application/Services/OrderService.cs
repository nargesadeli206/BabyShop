using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBasketRepository _basketRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IBasketRepository basketRepository,
        IProductRepository productRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _basketRepository = basketRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null) return null;
        return MapToDto(order);
    }

    public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.FindAsync(o => o.UserId == userId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new Order(dto.UserId, dto.ShippingAddress, dto.PhoneNumber, dto.Email ?? "");

        if (!string.IsNullOrEmpty(dto.CustomerNote))
            order.AddCustomerNote(dto.CustomerNote);

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Product {item.ProductId} not found");

            order.AddItem(item.ProductId, product.Name, item.Quantity, item.UnitPrice);
        }

        var createdOrder = await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order created with ID: {OrderId}", createdOrder.Id);

        return MapToDto(createdOrder);
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new Exception("Order not found");

        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            ItemsCount = order.ItemsCount
        };
    }

    public async Task<List<OrderSummaryDto>> GetUserOrderSummariesAsync(int userId)
    {
        var orders = await _orderRepository.FindAsync(o => o.UserId == userId);
        return orders.Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            ItemsCount = o.ItemsCount
        }).ToList();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(dto.OrderId);
        if (order == null) throw new Exception("Order not found");

        switch (dto.Status.ToLower())
        {
            case "paid":
                order.MarkAsPaid();
                break;
            case "shipped":
                order.MarkAsShipped();
                break;
            case "delivered":
                order.MarkAsDelivered();
                break;
            case "cancelled":
                order.Cancel(dto.Reason ?? "No reason provided");
                break;
            default:
                throw new Exception($"Invalid status: {dto.Status}");
        }

        await _orderRepository.UpdateAsync(order);
        return MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderFromBasketAsync(CreateOrderFromBasketDto dto)
    {
        var basket = await _basketRepository.GetBasketWithItemsAsync(dto.BasketId);
        if (basket == null || basket.Items == null || !basket.Items.Any())
            throw new Exception("Basket is empty or not found");

        var order = new Order(dto.UserId, dto.ShippingAddress, dto.PhoneNumber, dto.Email ?? "");

        if (!string.IsNullOrEmpty(dto.CustomerNote))
            order.AddCustomerNote(dto.CustomerNote);

        foreach (var item in basket.Items)
        {
            order.AddItem(item.ProductId, item.Product?.Name ?? "Unknown", item.Quantity, item.UnitPrice);
        }

        if (basket.DiscountAmount.HasValue)
            order.ApplyDiscount(basket.DiscountAmount.Value);

        var createdOrder = await _orderRepository.AddAsync(order);

        // Clear the basket after creating order
        await _basketRepository.ClearBasketAsync(dto.BasketId);

        _logger.LogInformation("Order created from basket {BasketId} with ID: {OrderId}", dto.BasketId, createdOrder.Id);

        return MapToDto(createdOrder);
    }

    public async Task<OrderDto> CancelOrderAsync(int orderId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new Exception("Order not found");

        order.Cancel(reason);
        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} cancelled. Reason: {Reason}", orderId, reason);

        return MapToDto(order);
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserName = order.User?.FullName ?? string.Empty,
            ShippingAddress = order.ShippingAddress,
            PhoneNumber = order.PhoneNumber,
            Email = order.Email,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            Discount = order.Discount,
            TotalAmount = order.TotalAmount,
            CustomerNote = order.CustomerNote,
            AdminNote = order.AdminNote,
            PaidAt = order.PaidAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}