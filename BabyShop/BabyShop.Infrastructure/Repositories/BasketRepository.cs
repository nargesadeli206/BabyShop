using BabyShop.Application.Interfaces.Repositories;
using BabyShop.Core.Entities;
using BabyShop.Core.Enums;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly AppDbContext _context;

    public BasketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Basket?> GetByIdAsync(int id)
    {
        return await _context.Baskets
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Basket?> GetActiveBasketByUserIdAsync(int userId)
    {
        return await _context.Baskets
            .Include(b => b.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Status == BasketStatus.Active);
    }

    public async Task<Basket?> GetActiveBasketBySessionIdAsync(string sessionId)
    {
        return await _context.Baskets
            .Include(b => b.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(b => b.SessionId == sessionId && b.Status == BasketStatus.Active);
    }

    public async Task<Basket?> GetBasketWithItemsAsync(int basketId)
    {
        return await _context.Baskets
            .Include(b => b.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(b => b.Id == basketId);
    }

    public async Task<BasketItem?> GetBasketItemByIdAsync(int basketItemId)
    {
        return await _context.BasketItems
            .Include(bi => bi.Product)
            .FirstOrDefaultAsync(bi => bi.Id == basketItemId);
    }

    public async Task<BasketItem?> GetBasketItemByProductAsync(int basketId, int productId)
    {
        return await _context.BasketItems
            .FirstOrDefaultAsync(bi => bi.BasketId == basketId && bi.ProductId == productId);
    }

    public async Task<bool> BasketExistsAsync(int basketId)
    {
        return await _context.Baskets.AnyAsync(b => b.Id == basketId);
    }

    public async Task<Basket> CreateAsync(Basket basket)
    {
        await _context.Baskets.AddAsync(basket);
        await SaveChangesAsync();
        return basket;
    }

    public async Task AddBasketItemAsync(BasketItem basketItem)
    {
        await _context.BasketItems.AddAsync(basketItem);
        await SaveChangesAsync();
    }

    public async Task UpdateBasketItemAsync(BasketItem basketItem)
    {
        _context.BasketItems.Update(basketItem);
        await SaveChangesAsync();
    }

    public async Task<bool> RemoveBasketItemAsync(int basketItemId)
    {
        var basketItem = await GetBasketItemByIdAsync(basketItemId);
        if (basketItem == null)
            return false;

        _context.BasketItems.Remove(basketItem);
        await SaveChangesAsync();
        return true;
    }

    public async Task ClearBasketAsync(int basketId)
    {
        var basket = await GetBasketWithItemsAsync(basketId);
        if (basket != null && basket.Items.Any())
        {
            _context.BasketItems.RemoveRange(basket.Items);
            await SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Basket basket)
    {
        _context.Baskets.Update(basket);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(int basketId)
    {
        var basket = await GetByIdAsync(basketId);
        if (basket != null)
        {
            _context.Baskets.Remove(basket);
            await SaveChangesAsync();
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}