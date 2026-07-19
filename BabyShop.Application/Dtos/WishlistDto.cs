namespace BabyShop.Application.Dtos;

public class WishlistDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? ProductImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddToWishlistDto
{
    public int ProductId { get; set; }
}