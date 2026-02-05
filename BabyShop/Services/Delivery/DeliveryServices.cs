using BabyShop.Interfaces;
using BabyShop.Repositories;

namespace BabyShop.Services.Deliveries
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _repository;

        public DeliveryService(IDeliveryRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync()
        {
        
        }
    }
}