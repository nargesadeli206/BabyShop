using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyShop.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IInventoryService inventoryService,
            IBasketRepository basketRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _inventoryService = inventoryService;
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<List<OrderSummaryDto>> GetAllOrdersForAdminAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                UserName = o.UserName ?? "مشتری مهمان",
                PhoneNumber = o.PhoneNumber,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Order(dto.UserId, dto.ShippingAddress, dto.PhoneNumber, dto.Email ?? "");

            if (!string.IsNullOrEmpty(dto.CustomerNote))
                order.CustomerNote = dto.CustomerNote;

            foreach (var item in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null) throw new Exception("محصول یافت نشد");

                order.AddItem(item.ProductId, product.Name, item.Quantity, item.UnitPrice);

                // کسر از انبار
                await _inventoryService.DecreaseStockAsync(item.ProductId, item.Quantity);
            }

            var createdOrder = await _orderRepository.AddAsync(order);
            return MapToDto(createdOrder);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            switch (dto.Status.ToLower())
            {
                case "paid": order.MarkAsPaid(); break;
                case "shipped": order.MarkAsShipped(); break;
                case "delivered": order.MarkAsDelivered(); break;
                case "cancelled": order.Cancel(dto.Reason ?? "No reason"); break;
            }

            await _orderRepository.UpdateAsync(order);
            return MapToDto(order);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<List<OrderSummaryDto>> GetUserOrderSummariesAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserAsync(userId);
            return orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            }).ToList();
        }

        public async Task<OrderDto> CreateOrderFromBasketAsync(CreateOrderFromBasketDto dto)
        {
            var basket = await _basketRepository.GetBasketWithItemsAsync(dto.BasketId);
            if (basket == null) throw new Exception("سبد یافت نشد");

            var order = new Order(basket.UserId, dto.ShippingAddress, dto.PhoneNumber, dto.Email ?? "");
            foreach (var item in basket.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                order.AddItem(item.ProductId, product?.Name ?? "کالا", item.Quantity, item.UnitPrice);
                await _inventoryService.DecreaseStockAsync(item.ProductId, item.Quantity);
            }

            var createdOrder = await _orderRepository.AddAsync(order);
            await _basketRepository.ClearBasketAsync(basket.Id);
            return MapToDto(createdOrder);
        }

        public async Task<OrderDto> CancelOrderAsync(int orderId, string reason)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Cancel(reason);
                await _orderRepository.UpdateAsync(order);
            }
            return MapToDto(order!);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UserName = order.UserName ?? "",
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                CustomerNote = order.CustomerNote,

                // این خط دقیقاً همونیه که لیست لباس‌ها رو می‌فرسته تو فاکتور!
                Items = order.Items?.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList() ?? new List<OrderItemDto>()
            };
        }

        public Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId) => throw new NotImplementedException();
        public Task<OrderSummaryDto> GetOrderSummaryAsync(int orderId) => throw new NotImplementedException();
    }
}