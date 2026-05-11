namespace BabyShop.Application.Dtos.Basket;

public class BasketDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? SessionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BasketItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class BasketSummaryDto
{
    public int BasketId { get; set; }
    public int TotalItems { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public bool HasDiscount { get; set; }
    public string? DiscountCode { get; set; }
}

public class BasketOperationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BasketDto? Basket { get; set; }
}

public class AddToBasketDto
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBasketItemDto
{
    public int BasketItemId { get; set; }
    public int Quantity { get; set; }
}

public class ApplyDiscountDto
{
    public int BasketId { get; set; }
    public string DiscountCode { get; set; } = string.Empty;
}

public class MergeBasketDto
{
    public int AnonymousUserId { get; set; }
    public int RegisteredUserId { get; set; }
}