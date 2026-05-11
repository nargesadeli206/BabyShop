using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IBasketRepository
{
    // متدهای پایه
    Task<Basket?> GetByIdAsync(int id);
    Task<IReadOnlyList<Basket>> GetAllAsync();
    Task<Basket> AddAsync(Basket entity);
    Task UpdateAsync(Basket entity);
    Task DeleteAsync(Basket entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();

    // متدهای اختصاصی Basket
    Task<Basket?> GetActiveBasketByUserIdAsync(int userId);
    Task<Basket?> GetBasketWithItemsAsync(int basketId);
    Task<Basket?> GetOrCreateActiveBasketAsync(int userId);
    Task<BasketItem?> GetBasketItemByProductAsync(int basketId, int productId);
    Task<BasketItem?> GetBasketItemByIdAsync(int id);
    Task AddBasketItemAsync(BasketItem item);
    Task UpdateBasketItemAsync(BasketItem item);
    Task<bool> RemoveBasketItemAsync(int basketItemId);
    Task<bool> ClearBasketAsync(int basketId);
    Task<int> GetBasketItemsCountAsync(int userId);
    Task<bool> ProductExistsAsync(int productId);
    Task<Basket> AddToBasketAsync(int userId, int productId, int quantity);
    Task<Basket> UpdateBasketItemAsync(int basketItemId, int quantity);
    Task<Basket> ApplyDiscountAsync(int basketId, string discountCode);
    Task<bool> RemoveDiscountAsync(int basketId);
}