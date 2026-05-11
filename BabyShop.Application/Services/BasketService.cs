using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _basketRepository;
    private readonly ILogger<BasketService> _logger;

    public BasketService(IBasketRepository basketRepository, ILogger<BasketService> logger)
    {
        _basketRepository = basketRepository;
        _logger = logger;
    }

    public async Task<BasketDto?> GetBasketByUserIdAsync(int userId)
    {
        var basket = await _basketRepository.GetActiveBasketByUserIdAsync(userId);
        if (basket == null) return null;
        return MapToDto(basket);
    }

    public async Task<int> GetBasketItemsCountAsync(int userId)
    {
        return await _basketRepository.GetBasketItemsCountAsync(userId);
    }

    public async Task<BasketDto> AddToBasketAsync(int userId, int productId, int quantity)
    {
        var basket = await _basketRepository.AddToBasketAsync(userId, productId, quantity);
        _logger.LogInformation("Added product {ProductId} to basket for user {UserId}", productId, userId);
        return MapToDto(basket);
    }

    public async Task<BasketDto> UpdateBasketItemAsync(int basketItemId, int quantity)
    {
        var basket = await _basketRepository.UpdateBasketItemAsync(basketItemId, quantity);
        return MapToDto(basket);
    }

    public async Task<BasketDto> ApplyDiscountAsync(int basketId, string discountCode)
    {
        var basket = await _basketRepository.ApplyDiscountAsync(basketId, discountCode);
        return MapToDto(basket);
    }

    public async Task<bool> RemoveDiscountAsync(int basketId)
    {
        return await _basketRepository.RemoveDiscountAsync(basketId);
    }

    public async Task<bool> ClearBasketAsync(int basketId)
    {
        return await _basketRepository.ClearBasketAsync(basketId);
    }

    public async Task<bool> RemoveBasketItemAsync(int basketItemId)
    {
        return await _basketRepository.RemoveBasketItemAsync(basketItemId);
    }

    private BasketDto MapToDto(Basket basket)
    {
        return new BasketDto
        {
            Id = basket.Id,
            UserId = basket.UserId,
            Status = basket.Status,
            DiscountCode = basket.DiscountCode,
            DiscountAmount = basket.DiscountAmount,
            SubTotal = basket.SubTotal,
            TotalPrice = basket.TotalPrice,
            TotalItems = basket.TotalItems,
            CreatedAt = basket.CreatedAt,
            UpdatedAt = basket.UpdatedAt,
            Items = basket.Items?.Select(item => new BasketItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.UnitPrice * item.Quantity
            }).ToList() ?? new List<BasketItemDto>()
        };
    }
}