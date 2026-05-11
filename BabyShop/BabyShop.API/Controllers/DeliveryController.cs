using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

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

    [HttpPost]
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
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DeliveryDto>>> Get(int id)
    {
        var delivery = await _deliveryService.GetDeliveryByIdAsync(id);
        if (delivery == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Delivery {id} not found" });

        return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<DeliveryDto>>> GetByOrder(int orderId)
    {
        var delivery = await _deliveryService.GetDeliveryByOrderIdAsync(orderId);
        if (delivery == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Delivery for order {orderId} not found" });

        return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<DeliveryDto>>> UpdateStatus([FromBody] UpdateDeliveryStatusDto dto)
    {
        try
        {
            var delivery = await _deliveryService.UpdateDeliveryStatusAsync(dto);
            return Ok(new ApiResponse<DeliveryDto> { Success = true, Data = delivery });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<DeliveryDto>>>> GetPending()
    {
        var deliveries = await _deliveryService.GetPendingDeliveriesAsync();
        return Ok(new ApiResponse<List<DeliveryDto>> { Success = true, Data = deliveries });
    }
}