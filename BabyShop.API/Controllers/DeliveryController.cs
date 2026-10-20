using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.API.Controllers
{
    // قفل مدیر رو از روی کل کلاس برداشتیم تا کاربر عادی هم بتونه بعضی چیزا رو ببینه
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(IDeliveryService deliveryService, ILogger<DeliveryController> logger)
        {
            _deliveryService = deliveryService;
            _logger = logger;
        }

        // ایجاد دلیوری رو قفل کردیم فقط برای ادمین
        [HttpPost]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<ActionResult<ApiResponse<DeliveryDto>>> Create([FromBody] CreateDeliveryDto dto)
        {
            try
            {
                var delivery = await _deliveryService.CreateDeliveryAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = delivery.Id },
                    new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
            }
            catch (NotFoundException ex)
            {
                return Ok(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return Ok(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DeliveryDto>>> Get(int id)
        {
            var delivery = await _deliveryService.GetDeliveryDtoByIdAsync(id);
            if (delivery == null)
                return Ok(new ApiResponse<object> { Success = false, Message = $"Delivery {id} not found" });

            return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
        }

        // متدی که فرانت‌اِند برای پیدا کردن کد رهگیری بهش نیاز داره (آزاد برای کاربری که لاگین کرده)
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ApiResponse<DeliveryDto>>> GetByOrder(int orderId)
        {
            var delivery = await _deliveryService.GetDeliveryDtoByOrderIdAsync(orderId);
            if (delivery == null)
                return Ok(new ApiResponse<object> { Success = false, Message = $"سفارشی برای این دلیوری یافت نشد." });

            return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
        }

        // آپدیت وضعیت رو هم فقط به ادمین دادیم
        [HttpPut("status")]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<ActionResult<ApiResponse<DeliveryDto>>> UpdateStatus([FromBody] UpdateDeliveryStatusDto dto)
        {
            try
            {
                var delivery = await _deliveryService.UpdateDeliveryStatusAsync(dto);
                return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
            }
            catch (NotFoundException ex)
            {
                return Ok(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (BusinessRuleException ex)
            {
                return Ok(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("pending")]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<ActionResult<ApiResponse<List<DeliveryDto>>>> GetPending()
        {
            var deliveries = await _deliveryService.GetPendingDeliveryDtosAsync();
            return Ok(new ApiResponse<List<DeliveryDto>> { Success = true, Data = deliveries });
        }
    }
}