using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.Entities.Inventory;
using BabyShop.Services.BabyShop.Application.Dtos.Inventory;

namespace BabyShop.BabyShop.Application.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(CreateInventoryDto dto)
        {
            var item = new InventoryItem(dto.ProductId, dto.Stock);
            await _repository.AddAsync(item);
        }

        public async Task IncreaseAsync(UpdateInventoryDto dto)
        {
            var item = await _repository.GetByProductIdAsync(dto.ProductId);
            if (item == null)
                throw new Exception("Inventory not found");

            item.Increase(dto.Quantity);
            await _repository.UpdateAsync(item);
        }

        public async Task DecreaseAsync(UpdateInventoryDto dto)
        {
            var item = await _repository.GetByProductIdAsync(dto.ProductId);
            if (item == null)
                throw new Exception("Inventory not found");

            item.Decrease(dto.Quantity);
            await _repository.UpdateAsync(item);
        }

        public async Task<InventoryDto?> GetByProductIdAsync(int productId)
        {
            var item = await _repository.GetByProductIdAsync(productId);
            if (item == null) return null;

            return new InventoryDto
            {
                ProductId = item.ProductId,
                Stock = item.Stock
            };
        }
    }
}