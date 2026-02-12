using BabyShop.Services.BabyShop.Application.Dtos.Payment;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<PaymentResponseDto>> Create([FromBody] PaymentCreateDto dto)
    {
        var payment = await _paymentService.CreatePaymentAsync(dto.OrderId, dto.Amount);
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            Authority = payment.Authority
        };
    }

    [HttpPost("verify")]
    public async Task<ActionResult> Verify([FromQuery] string authority)
    {
        var success = await _paymentService.VerifyAsync(authority);
        if (!success) return BadRequest("Payment not found or failed");
        return Ok("Payment verified successfully");
    }
}