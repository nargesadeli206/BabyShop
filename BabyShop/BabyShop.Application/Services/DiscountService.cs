namespace BabyShop.Application.Interfaces.Services;

public interface DiscountService
{
    Task<DiscountValidationResult> ValidateDiscountAsync(string discountCode, decimal basketTotal);
}

public class DiscountValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
}