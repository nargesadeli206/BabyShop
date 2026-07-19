using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface IWishlistRepository
{
    Task<IReadOnlyList<Wishlist>> GetWishlistByUserIdAsync(int userId);
    Task<Wishlist?> GetWishlistItemAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
    Task<Wishlist> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<int> GetWishlistCountAsync(int userId);
    Task ClearWishlistAsync(int userId);
}