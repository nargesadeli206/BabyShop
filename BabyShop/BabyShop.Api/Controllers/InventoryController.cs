using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.Services.BabyShop.Application.Dtos.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.BabyShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInventoryDto dto)
        {
            await _service.CreateAsync(dto);
            return Ok();
        }

        [HttpPut("increase")]
        public async Task<IActionResult> Increase(UpdateInventoryDto dto)
        {
            await _service.IncreaseAsync(dto);
            return Ok();
        }

        [HttpPut("decrease")]
        public async Task<IActionResult> Decrease(UpdateInventoryDto dto)
        {
            await _service.DecreaseAsync(dto);
            return Ok();
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> Get(int productId)
        {
            var result = await _service.GetByProductIdAsync(productId);
            return result == null ? NotFound() : Ok(result);
        }
    }
}

