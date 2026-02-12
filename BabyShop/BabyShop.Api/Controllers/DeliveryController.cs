using BabyShop.BabyShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.BabyShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _service;

        public DeliveryController(IDeliveryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDeliveryDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(id);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var delivery = await _service.GetByIdAsync(id);
            return delivery == null ? NotFound() : Ok(delivery);
        }
    }
}