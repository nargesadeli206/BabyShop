using Microsoft.AspNetCore.Mvc;
using BabyShop.Entities;
using BabyShop.Interfaces;

namespace BabyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _repository;

        public InventoryController(IInventoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repository.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            await _repository.AddAsync(inventory);
            return Ok(inventory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Inventory inventory)
        {
            var old = await _repository.GetByIdAsync(id);
            if (old == null) return NotFound();

            old.ProductId = inventory.ProductId;
            old.Count = inventory.Count;
            old.UpdatedAt = DateTime.Now;

            await _repository.UpdateAsync(old);
            return Ok(old);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            await _repository.DeleteAsync(item);
            return Ok();
        }
    }
}