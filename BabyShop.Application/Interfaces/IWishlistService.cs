using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IWishlistService
{
    Task<IReadOnlyList<WishlistDto>> GetWishlistAsync(int userId);
    Task<WishlistDto> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
    Task<int> GetWishlistCountAsync(int userId);
    Task ClearWishlistAsync(int userId);
}