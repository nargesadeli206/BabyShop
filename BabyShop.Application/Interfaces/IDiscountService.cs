namespace BabyShop.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<decimal> CalculateDiscountAsync(decimal amount, string? discountCode);
}