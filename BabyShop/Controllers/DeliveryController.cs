using BabyShop.Dtos;
using BabyShop.DTOs;
using BabyShop.Entities;
using BabyShop.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryRepository _repository;

        public DeliveryController(IDeliveryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deliveries = await _repository.GetAllAsync();
            return Ok(deliveries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return NotFound();

            return Ok(delivery);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeliveryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var delivery = new Delivery
            {
                Address = dto.Address,
                Price = dto.Price,
                PhoneNumber = dto.PhoneNumber,
                PostalCode = dto.PostalCode
            };

            await _repository.AddAsync(delivery);

            return CreatedAtAction(nameof(GetById), new { id = delivery.Id }, delivery);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var delivery = await _repository.GetByIdAsync(id);
            if (delivery == null)
                return NotFound();

            await _repository.DeleteAsync(id);

            return NoContent();
        }
    }
}   