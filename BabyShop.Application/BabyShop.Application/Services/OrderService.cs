using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryService inventoryService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new BusinessRuleException("Order must have at least one item");
        if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
            throw new BusinessRuleException("Shipping address is required");

        // Check stock and reserve
        foreach (var item in dto.Items)
        {
            var hasStock = await _inventoryService.HasEnoughStockAsync(item.ProductId, item.Quantity);
            if (!hasStock)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                throw new BusinessRuleException($"Product '{product?.Name}' does not have enough stock");
            }
        }

        var order = new Order(dto.UserId, dto.ShippingAddress, dto.PhoneNumber, dto.Email);

        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);

            // Reserve stock
            await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity);

            order.AddItem(
                item.ProductId,
                product?.Name ?? "Unknown",
                item.Quantity,
                item.Price);
        }

        await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order created with ID: {OrderId}, Number: {OrderNumber}", order.Id, order.OrderNumber);

        return await MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(id);
        if (order == null || order.IsDeleted)
            return null;

        return await MapToOrderDto(order);
    }

    public async Task<PagedResultDto<OrderSummaryDto>> GetOrdersPagedAsync(int pageNumber, int pageSize)
    {
        var orders = await _orderRepository.GetPagedAsync(pageNumber, pageSize);
        var totalCount = await _orderRepository.CountAsync();
        var items = new List<OrderSummaryDto>();

        foreach (var order in orders.Where(o => !o.IsDeleted))
        {
            items.Add(new OrderSummaryDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ItemsCount = order.Items.Count
            });
        }

        return new PagedResultDto<OrderSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<OrderSummaryDto>> GetOrdersByUserAsync(int userId)
    {
        var orders = await _orderRepository.GetOrdersByUserAsync(userId);
        var result = new List<OrderSummaryDto>();

        foreach (var order in orders.Where(o => !o.IsDeleted))
        {
            result.Add(new OrderSummaryDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ItemsCount = order.Items.Count
            });
        }

        return result;
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(dto.OrderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), dto.OrderId);

        switch (dto.Status.ToLower())
        {
            case "paid":
                order.MarkAsPaid();
                // Release reserved stock and decrease actual stock
                foreach (var item in order.Items)
                {
                    await _inventoryService.DecreaseStockAsync(new UpdateInventoryDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
                break;
            case "shipped":
                order.MarkAsShipped();
                break;
            case "delivered":
                order.MarkAsDelivered();
                break;
            case "cancelled":
                order.Cancel(dto.Reason ?? "Cancelled by admin");
                // Release reserved stock
                foreach (var item in order.Items)
                {
                    await _inventoryService.ReleaseReservedStockAsync(item.ProductId, item.Quantity);
                }
                break;
            default:
                throw new BusinessRuleException($"Invalid status: {dto.Status}");
        }

        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, dto.Status);

        return await MapToOrderDto(order);
    }

    public async Task CancelOrderAsync(int orderId, string reason)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null)
            throw new NotFoundException(nameof(Order), orderId);

        order.Cancel(reason);

        // Release reserved stock
        foreach (var item in order.Items)
        {
            await _inventoryService.ReleaseReservedStockAsync(item.ProductId, item.Quantity);
        }

        await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Order {OrderId} cancelled", orderId);
    }

    private async Task<OrderDto> MapToOrderDto(Order order)
    {
        var orderDto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            Discount = order.Discount,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            PhoneNumber = order.PhoneNumber,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            Items = new List<OrderItemDto>()
        };

        foreach (var item in order.Items)
        {
            orderDto.Items.Add(new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            });
        }

        return orderDto;
    }
}