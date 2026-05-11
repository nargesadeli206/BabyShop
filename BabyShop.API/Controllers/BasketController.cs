using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

// [Authorize]  ← کامنت شد برای تست
[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        // مقدار ثابت برای تست
        return 19;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBasket()
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.GetBasketByUserIdAsync(userId);
            return Ok(new ApiResponse<BasketDto?> { Success = true, Data = basket });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpGet("my/count")]
    public async Task<IActionResult> GetBasketItemsCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _basketService.GetBasketItemsCountAsync(userId);
            return Ok(new ApiResponse<int> { Success = true, Data = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket count");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToBasket([FromQuery] int productId, [FromQuery] int quantity = 1)
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.AddToBasketAsync(userId, productId, quantity);
            return Ok(new ApiResponse<BasketDto> { Success = true, Data = basket, Message = "Product added to basket" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to basket");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpPut("items/{basketItemId}")]
    public async Task<IActionResult> UpdateBasketItem(int basketItemId, [FromQuery] int quantity)
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.UpdateBasketItemAsync(basketItemId, quantity);
            return Ok(new ApiResponse<BasketDto> { Success = true, Data = basket, Message = "Basket updated" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket item");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpDelete("items/{basketItemId}")]
    public async Task<IActionResult> RemoveBasketItem(int basketItemId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _basketService.RemoveBasketItemAsync(basketItemId);
            return Ok(new ApiResponse<bool> { Success = true, Data = result, Message = "Item removed from basket" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing basket item");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearBasket()
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.GetBasketByUserIdAsync(userId);
            if (basket == null)
                return Ok(new ApiResponse<bool> { Success = true, Data = true, Message = "Basket is already empty" });

            var result = await _basketService.ClearBasketAsync(basket.Id);
            return Ok(new ApiResponse<bool> { Success = true, Data = result, Message = "Basket cleared" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpPost("discount")]
    public async Task<IActionResult> ApplyDiscount([FromQuery] string discountCode)
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.GetBasketByUserIdAsync(userId);
            if (basket == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Basket not found" });

            var updatedBasket = await _basketService.ApplyDiscountAsync(basket.Id, discountCode);
            return Ok(new ApiResponse<BasketDto> { Success = true, Data = updatedBasket, Message = "Discount applied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpDelete("discount")]
    public async Task<IActionResult> RemoveDiscount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var basket = await _basketService.GetBasketByUserIdAsync(userId);
            if (basket == null)
                return Ok(new ApiResponse<bool> { Success = true, Data = true, Message = "No discount to remove" });

            var result = await _basketService.RemoveDiscountAsync(basket.Id);
            return Ok(new ApiResponse<bool> { Success = true, Data = result, Message = "Discount removed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing discount");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }
}