using BabyShop.Application.Dtos.Basket;

namespace BabyShop.Application.Interfaces.Services;

public interface IBasketService
{
    // Get Operations
    Task<BasketDto> GetBasketByUserIdAsync(int userId);
    Task<BasketDto> GetBasketBySessionIdAsync(string sessionId);
    Task<BasketSummaryDto> GetBasketSummaryAsync(int userId);
    Task<int> GetBasketItemsCountAsync(int userId);

    // Write Operations
    Task<BasketOperationResponseDto> AddToBasketAsync(AddToBasketDto dto);
    Task<BasketOperationResponseDto> UpdateBasketItemAsync(UpdateBasketItemDto dto);
    Task<BasketOperationResponseDto> RemoveFromBasketAsync(int basketItemId);
    Task<BasketOperationResponseDto> ClearBasketAsync(int userId);

    // Discount Operations
    Task<BasketOperationResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto);
    Task<BasketOperationResponseDto> RemoveDiscountAsync(int basketId);

    // Merge Operation
    Task<BasketOperationResponseDto> MergeBasketAsync(MergeBasketDto dto);

    // Checkout Operation
    Task<BasketDto> PrepareForCheckoutAsync(int basketId);
}