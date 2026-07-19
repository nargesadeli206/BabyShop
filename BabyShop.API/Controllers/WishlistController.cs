using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BabyShop.API.Controllers;

[Authorize] 
[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated");
        return int.Parse(userIdClaim);
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        try
        {
            var userId = GetCurrentUserId();
            var wishlist = await _wishlistService.GetWishlistAsync(userId);
            return Ok(new ApiResponse<IReadOnlyList<WishlistDto>> { Success = true, Data = wishlist });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetWishlistCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _wishlistService.GetWishlistCountAsync(userId);
            return Ok(new ApiResponse<int> { Success = true, Data = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist count");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> IsInWishlist(int productId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(new ApiResponse<bool> { Success = true, Data = isInWishlist });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking wishlist");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var wishlistItem = await _wishlistService.AddToWishlistAsync(userId, dto.ProductId);
            return Ok(new ApiResponse<WishlistDto> { Success = true, Data = wishlistItem, Message = "محصول به علاقه‌مندی‌ها اضافه شد" });
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
            _logger.LogError(ex, "Error adding to wishlist");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!result)
                return NotFound(new ApiResponse<object> { Success = false, Message = "محصول در علاقه‌مندی‌ها یافت نشد" });

            return Ok(new ApiResponse<bool> { Success = true, Data = result, Message = "محصول از علاقه‌مندی‌ها حذف شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from wishlist");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ClearWishlist()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _wishlistService.ClearWishlistAsync(userId);
            return Ok(new ApiResponse<bool> { Success = true, Data = true, Message = "علاقه‌مندی‌ها خالی شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }
}