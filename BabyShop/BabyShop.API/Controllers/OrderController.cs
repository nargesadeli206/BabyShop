using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = order.Id },
                new ApiResponse<OrderDto> { Success = true, Data = order });
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
    public async Task<ActionResult<ApiResponse<OrderDto>>> Get(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Order {id} not found" });

        return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
    }

    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<OrderSummaryDto>>>> GetPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetOrdersPagedAsync(pageNumber, pageSize);
        return Ok(new ApiResponse<PagedResultDto<OrderSummaryDto>> { Success = true, Data = result });
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<OrderSummaryDto>>>> GetByUser(int userId)
    {
        var orders = await _orderService.GetOrdersByUserAsync(userId);
        return Ok(new ApiResponse<List<OrderSummaryDto>> { Success = true, Data = orders });
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus([FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(dto);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
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

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(int id, [FromBody] string reason)
    {
        try
        {
            await _orderService.CancelOrderAsync(id, reason);
            return Ok(new ApiResponse<object> { Success = true, Message = "Order cancelled" });
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

    /// <summary>
    /// ایجاد سفارش از سبد خرید کاربر
    /// </summary>
    [HttpPost("from-basket/{userId}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrderFromBasket(
        int userId,
        [FromBody] CreateOrderFromBasketDto dto)
    {
        try
        {
            var order = await _orderService.CreateOrderFromBasketAsync(userId, dto);
            return CreatedAtAction(nameof(Get), new { id = order.Id },
                new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order,
                    Message = "سفارش با موفقیت ثبت شد"
                });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ثبت سفارش از سبد خرید برای کاربر {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور رخ داده است"
            });
        }
    }
}