using BabyShop.Dtos;
using BabyShop.Entities;
using BabyShop.Interfaces;

namespace BabyShop.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task UpdateAsync(UpdateInventoryDto dto)
        {
            if (dto.Quantity == 0)
                throw new Exception("Quantity cannot be zero");

            var inventory = await _repository.GetByProductIdAsync(dto.ProductId);

            if (inventory == null)
            {
                if (dto.Quantity < 0)
                    throw new Exception("Cannot reduce inventory that does not exist");

                inventory = new Inventory
                {
                    ProductId = dto.ProductId,
                    Count = dto.Quantity,
                    UpdatedAt = DateTime.Now
                };

                await _repository.AddAsync(inventory);
                return;
            }

            var newQuantity = inventory.Count + dto.Quantity;

            if (newQuantity < 0)
                throw new Exception("Inventory cannot be negative");

            inventory.Count = newQuantity;
            inventory.UpdatedAt = DateTime.Now;

            await _repository.UpdateAsync(inventory);
        }
    }

    public interface IInventoryService
    {
        Task UpdateAsync(UpdateInventoryDto dto);
    }
}