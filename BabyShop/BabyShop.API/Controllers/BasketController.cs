using BabyShop.Application.Dtos;
using BabyShop.Application.Dtos.Basket;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت سبد خرید کاربر (احراز هویت شده)
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <returns>سبد خرید کاربر</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<BasketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketDto>>> GetUserBasket(int userId)
    {
        try
        {
            var basket = await _basketService.GetBasketByUserIdAsync(userId);
            return Ok(new ApiResponse<BasketDto> { Success = true, Data = basket });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// دریافت سبد خرید با SessionId (برای کاربران مهمان)
    /// </summary>
    /// <param name="sessionId">شناسه جلسه</param>
    /// <returns>سبد خرید مهمان</returns>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<BasketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketDto>>> GetSessionBasket(string sessionId)
    {
        try
        {
            var basket = await _basketService.GetBasketBySessionIdAsync(sessionId);
            return Ok(new ApiResponse<BasketDto> { Success = true, Data = basket });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for session {SessionId}", sessionId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// دریافت خلاصه سبد خرید
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <returns>خلاصه سبد خرید</returns>
    [HttpGet("user/{userId}/summary")]
    [ProducesResponseType(typeof(ApiResponse<BasketSummaryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketSummaryDto>>> GetBasketSummary(int userId)
    {
        try
        {
            var summary = await _basketService.GetBasketSummaryAsync(userId);
            return Ok(new ApiResponse<BasketSummaryDto> { Success = true, Data = summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket summary for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// دریافت تعداد آیتم‌های سبد خرید
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <returns>تعداد آیتم‌ها</returns>
    [HttpGet("user/{userId}/count")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<int>>> GetBasketCount(int userId)
    {
        try
        {
            var count = await _basketService.GetBasketItemsCountAsync(userId);
            return Ok(new ApiResponse<int> { Success = true, Data = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket count for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// افزودن محصول به سبد خرید
    /// </summary>
    /// <param name="dto">اطلاعات محصول</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> AddToBasket([FromBody] AddToBasketDto dto)
    {
        try
        {
            if (dto.Quantity <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "تعداد محصول باید بیشتر از صفر باشد"
                });
            }

            var result = await _basketService.AddToBasketAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to basket", dto.ProductId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// بروزرسانی تعداد محصول در سبد خرید
    /// </summary>
    /// <param name="dto">اطلاعات بروزرسانی</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPut("items")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> UpdateBasketItem([FromBody] UpdateBasketItemDto dto)
    {
        try
        {
            var result = await _basketService.UpdateBasketItemAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket item {BasketItemId}", dto.BasketItemId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// حذف یک آیتم از سبد خرید
    /// </summary>
    /// <param name="basketItemId">شناسه آیتم سبد خرید</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("items/{basketItemId}")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> RemoveFromBasket(int basketItemId)
    {
        try
        {
            var result = await _basketService.RemoveFromBasketAsync(basketItemId);

            if (!result.Success)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing basket item {BasketItemId}", basketItemId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// خالی کردن کامل سبد خرید
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("user/{userId}/clear")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> ClearBasket(int userId)
    {
        try
        {
            var result = await _basketService.ClearBasketAsync(userId);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// اعمال کد تخفیف
    /// </summary>
    /// <param name="dto">اطلاعات کد تخفیف</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPost("apply-discount")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> ApplyDiscount([FromBody] ApplyDiscountDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.DiscountCode))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "کد تخفیف نمی‌تواند خالی باشد"
                });
            }

            var result = await _basketService.ApplyDiscountAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount {DiscountCode}", dto.DiscountCode);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// حذف کد تخفیف از سبد خرید
    /// </summary>
    /// <param name="basketId">شناسه سبد خرید</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("discount/{basketId}")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> RemoveDiscount(int basketId)
    {
        try
        {
            var result = await _basketService.RemoveDiscountAsync(basketId);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing discount from basket {BasketId}", basketId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// ادغام سبد خرید کاربر مهمان با کاربر ثبت‌نام شده
    /// </summary>
    /// <param name="dto">اطلاعات ادغام</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(ApiResponse<BasketOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketOperationResponseDto>>> MergeBasket([FromBody] MergeBasketDto dto)
    {
        try
        {
            var result = await _basketService.MergeBasketAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
            }

            return Ok(new ApiResponse<BasketOperationResponseDto>
            {
                Success = true,
                Data = result,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging baskets");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// آماده‌سازی سبد خرید برای تسویه حساب
    /// </summary>
    /// <param name="basketId">شناسه سبد خرید</param>
    /// <returns>سبد خرید آماده شده</returns>
    [HttpPost("{basketId}/prepare-checkout")]
    [ProducesResponseType(typeof(ApiResponse<BasketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<BasketDto>>> PrepareForCheckout(int basketId)
    {
        try
        {
            var basket = await _basketService.PrepareForCheckoutAsync(basketId);
            return Ok(new ApiResponse<BasketDto>
            {
                Success = true,
                Data = basket,
                Message = "سبد خرید برای تسویه حساب آماده شد"
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing basket {BasketId} for checkout", basketId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "خطای داخلی سرور",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}