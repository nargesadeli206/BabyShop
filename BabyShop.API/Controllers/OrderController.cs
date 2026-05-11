using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BabyShop.API.Controllers;

//[Authorize(Policy = "UserOnly")]
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

    private int GetCurrentUserId()
    {
        // برای تست بدون احراز هویت، مقدار ثابت برگردان
        return 19;  // ← مقدار ثابت برای تست
        // return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            dto.UserId = GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = order.Id },
                new ApiResponse<OrderDto> { Success = true, Data = order, Message = "Order created" });
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

        // بررسی مالکیت: فقط Admin یا خود کاربر
        if (!IsAdmin && order.UserId != GetCurrentUserId())
            return Forbid();

        return Ok(new ApiResponse<OrderDto> { Success = true, Data = order });
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<OrderSummaryDto>>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();
        var orders = await _orderService.GetUserOrderSummariesAsync(userId);
        return Ok(new ApiResponse<List<OrderSummaryDto>> { Success = true, Data = orders });
    }

    [HttpGet("my/latest")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> GetMyLatestOrder()
    {
        var userId = GetCurrentUserId();
        var orders = await _orderService.GetUserOrderSummariesAsync(userId);
        var latestOrder = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();

        if (latestOrder == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "No orders found" });

        return Ok(new ApiResponse<OrderSummaryDto> { Success = true, Data = latestOrder });
    }

    [HttpPut("status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus([FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(dto);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = order, Message = "Order status updated" });
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
    public async Task<ActionResult<ApiResponse<OrderDto>>> Cancel(int id, [FromBody] string reason)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"Order {id} not found" });

            // فقط خود کاربر یا Admin می‌تواند لغو کند
            if (!IsAdmin && order.UserId != GetCurrentUserId())
                return Forbid();

            var cancelledOrder = await _orderService.CancelOrderAsync(id, reason);
            return Ok(new ApiResponse<OrderDto> { Success = true, Data = cancelledOrder, Message = "Order cancelled" });
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

    [HttpPost("from-basket")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrderFromBasket([FromBody] CreateOrderFromBasketDto dto)
    {
        try
        {
            dto.UserId = GetCurrentUserId();
            var order = await _orderService.CreateOrderFromBasketAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = order.Id },
                new ApiResponse<OrderDto> { Success = true, Data = order, Message = "Order created from basket" });
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
            _logger.LogError(ex, "Error creating order from basket");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }
}