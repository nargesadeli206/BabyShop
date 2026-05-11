using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IBasketService
{
    Task<BasketDto?> GetBasketByUserIdAsync(int userId);
    Task<int> GetBasketItemsCountAsync(int userId);
    Task<BasketDto> AddToBasketAsync(int userId, int productId, int quantity);
    Task<BasketDto> UpdateBasketItemAsync(int basketItemId, int quantity);
    Task<BasketDto> ApplyDiscountAsync(int basketId, string discountCode);
    Task<bool> RemoveDiscountAsync(int basketId);
    Task<bool> ClearBasketAsync(int basketId);
    Task<bool> RemoveBasketItemAsync(int basketItemId);
}