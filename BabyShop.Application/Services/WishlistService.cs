using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;

namespace BabyShop.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;

    public WishlistService(IWishlistRepository wishlistRepository, IProductRepository productRepository)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<WishlistDto>> GetWishlistAsync(int userId)
    {
        var wishlistItems = await _wishlistRepository.GetWishlistByUserIdAsync(userId);

        return wishlistItems.Select(item => new WishlistDto
        {
            Id = item.Id,
            UserId = item.UserId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductSlug = item.Product?.Slug ?? string.Empty,
            ProductPrice = item.Product?.Price ?? 0,
            ProductImage = item.Product?.ImageUrl,
            CreatedAt = item.CreatedAt
        }).ToList();
    }

    public async Task<WishlistDto> AddToWishlistAsync(int userId, int productId)
    {
      
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException(nameof(Product), productId);

     
        var exists = await _wishlistRepository.IsInWishlistAsync(userId, productId);
        if (exists)
            throw new BusinessRuleException("این محصول قبلاً به علاقه‌مندی‌ها اضافه شده است");

        var wishlistItem = await _wishlistRepository.AddToWishlistAsync(userId, productId);

        return new WishlistDto
        {
            Id = wishlistItem.Id,
            UserId = wishlistItem.UserId,
            ProductId = wishlistItem.ProductId,
            ProductName = product.Name,
            ProductSlug = product.Slug,
            ProductPrice = product.Price,
            ProductImage = product.ImageUrl,
            CreatedAt = wishlistItem.CreatedAt
        };
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        return await _wishlistRepository.RemoveFromWishlistAsync(userId, productId);
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        return await _wishlistRepository.IsInWishlistAsync(userId, productId);
    }

    public async Task<int> GetWishlistCountAsync(int userId)
    {
        return await _wishlistRepository.GetWishlistCountAsync(userId);
    }

    public async Task ClearWishlistAsync(int userId)
    {
        await _wishlistRepository.ClearWishlistAsync(userId);
    }
}