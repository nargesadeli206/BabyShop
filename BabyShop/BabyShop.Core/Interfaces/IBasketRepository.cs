using BabyShop.Core.Entities;

namespace BabyShop.Application.Interfaces.Repositories;

public interface IBasketRepository
{
    // Read
    Task<Basket?> GetByIdAsync(int id);
    Task<Basket?> GetActiveBasketByUserIdAsync(int userId);
    Task<Basket?> GetActiveBasketBySessionIdAsync(string sessionId);
    Task<Basket?> GetBasketWithItemsAsync(int basketId);
    Task<BasketItem?> GetBasketItemByIdAsync(int basketItemId);
    Task<BasketItem?> GetBasketItemByProductAsync(int basketId, int productId);
    Task<bool> BasketExistsAsync(int basketId);

    // Write
    Task<Basket> CreateAsync(Basket basket);
    Task AddBasketItemAsync(BasketItem basketItem);
    Task UpdateBasketItemAsync(BasketItem basketItem);
    Task<bool> RemoveBasketItemAsync(int basketItemId);
    Task ClearBasketAsync(int basketId);
    Task UpdateAsync(Basket basket);
    Task DeleteAsync(int basketId);

    // Save
    Task<int> SaveChangesAsync();
}