using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(dto);
            return Ok(new ApiResponse<PaymentDto> { Success = true, Data = payment });
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

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetByOrder(int orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Payment for order {orderId} not found" });

        return Ok(new ApiResponse<PaymentDto> { Success = true, Data = payment });
    }

    [HttpGet("authority/{authority}")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> GetByAuthority(string authority)
    {
        var payment = await _paymentService.GetPaymentByAuthorityAsync(authority);
        if (payment == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Payment with authority {authority} not found" });

        return Ok(new ApiResponse<PaymentDto> { Success = true, Data = payment });
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Verify(int paymentId, string referenceNumber)
    {
        try
        {
            var payment = await _paymentService.VerifyPaymentAsync(paymentId, referenceNumber);
            return Ok(new ApiResponse<PaymentDto> { Success = true, Data = payment, Message = "Payment verified" });
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

    [HttpPost("{paymentId}/refund")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Refund(int paymentId, string reason)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(paymentId, reason);
            return Ok(new ApiResponse<PaymentDto> { Success = true, Data = payment, Message = "Payment refunded" });
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
}