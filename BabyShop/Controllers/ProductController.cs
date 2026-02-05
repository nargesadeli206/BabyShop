using BabyShop.Dtos;
using BabyShop.Entities;
using BabyShop.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace BabyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductController(IProductRepository repository)
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
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk(List<CreateProductDto> products)
        {
            foreach (var dto in products)
            {
                await _repository.AddAsync(new Product
                {
                    Name = dto.Name,
                    Price = dto.Price
                });
            }

            return Ok();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            var old = await _repository.GetByIdAsync(id);
            if (old == null) return NotFound();

            old.Name = product.Name;
            old.Price = product.Price;

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