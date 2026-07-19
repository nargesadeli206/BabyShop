using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class Wishlist : BaseEntity
{
    public int UserId { get; private set; }
    public int ProductId { get; private set; }

    public virtual User? User { get; set; }
    public virtual Product? Product { get; set; }

    private Wishlist() { }

    public Wishlist(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
        CreatedAt = DateTime.UtcNow;
    }
}