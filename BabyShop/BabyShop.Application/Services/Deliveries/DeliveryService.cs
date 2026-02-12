using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.Entities.Delivery;
using BabyShop.Services.BabyShop.Application.Dtos.Deliveries;

namespace BabyShop.BabyShop.Application.Services.Deliveries
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _repository;

        public DeliveryService(IDeliveryRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateDeliveryDto dto)
        {
            var delivery = new DeliveryEntity(dto.OrderId, dto.Address, dto.DeliveryDate);
            await _repository.AddAsync(delivery);
            return delivery.Id;
        }

        public async Task<DeliveryDto?> GetByIdAsync(int id)
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null) return null;

            return new DeliveryDto
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                Address = delivery.Address,
                DeliveryDate = delivery.DeliveryDate
            };
        }
    }
}