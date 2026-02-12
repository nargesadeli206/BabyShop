using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.Entities.Order;
using BabyShop.Services.BabyShop.Application.Dtos.Order;

namespace BabyShop.BabyShop.Application.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateOrderDto dto)
        {
            var order = new OrderEntity(); 

            foreach (var item in dto.Items)
            {
                order.AddItem(item.ProductId, item.Quantity, item.Price);
            }

            await _repository.AddAsync(order);
            return order.Id;
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice()
            };
        }
    }
}