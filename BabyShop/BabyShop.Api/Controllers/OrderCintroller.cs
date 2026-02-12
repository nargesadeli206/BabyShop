using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.Services.BabyShop.Application.Dtos.Order;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.BabyShop.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var orderId = await _service.CreateAsync(dto);
            return Ok(orderId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _service.GetByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }
    }
}