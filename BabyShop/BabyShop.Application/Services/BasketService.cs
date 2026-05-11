using BabyShop.Application.Dtos.Basket;
using BabyShop.Application.Interfaces.Repositories;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Enums;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _basketRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<BasketService> _logger;

    public BasketService(
        IBasketRepository basketRepository,
        IProductRepository productRepository,
        ILogger<BasketService> logger)
    {
        _basketRepository = basketRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<BasketDto> GetBasketByUserIdAsync(int userId)
    {
        try
        {
            var basket = await _basketRepository.GetActiveBasketByUserIdAsync(userId);

            if (basket == null)
            {
                return new BasketDto
                {
                    UserId = userId,
                    Status = BasketStatus.Active.ToString(),
                    Items = new List<BasketItemDto>(),
                    SubTotal = 0,
                    TotalPrice = 0,
                    TotalItems = 0
                };
            }

            return MapToBasketDto(basket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BasketDto> GetBasketBySessionIdAsync(string sessionId)
    {
        try
        {
            var basket = await _basketRepository.GetActiveBasketBySessionIdAsync(sessionId);

            if (basket == null)
            {
                basket = new Basket
                {
                    SessionId = sessionId,
                    Status = BasketStatus.Active,
                    Items = new List<BasketItem>()
                };

                basket = await _basketRepository.CreateAsync(basket);
            }

            return MapToBasketDto(basket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket for session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<BasketSummaryDto> GetBasketSummaryAsync(int userId)
    {
        try
        {
            var basket = await _basketRepository.GetActiveBasketByUserIdAsync(userId);

            if (basket == null)
            {
                return new BasketSummaryDto
                {
                    BasketId = 0,
                    TotalItems = 0,
                    SubTotal = 0,
                    Total = 0,
                    HasDiscount = false
                };
            }

            return new BasketSummaryDto
            {
                BasketId = basket.Id,
                TotalItems = basket.TotalItems,
                SubTotal = basket.SubTotal,
                DiscountAmount = basket.DiscountAmount,
                Total = basket.TotalPrice,
                HasDiscount = basket.DiscountAmount > 0,
                DiscountCode = basket.DiscountCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket summary for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetBasketItemsCountAsync(int userId)
    {
        try
        {
            var basket = await _basketRepository.GetActiveBasketByUserIdAsync(userId);
            return basket?.TotalItems ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting basket count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BasketOperationResponseDto> AddToBasketAsync(AddToBasketDto dto)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            // Validation
            if (dto.Quantity <= 0)
            {
                response.Success = false;
                response.Message = "تعداد محصول باید بیشتر از صفر باشد";
                return response;
            }

            // Get product
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                response.Success = false;
                response.Message = "محصول یافت نشد";
                return response;
            }

            // Check stock
            if (product.StockQuantity < dto.Quantity)
            {
                response.Success = false;
                response.Message = $"موجودی محصول {product.Name} کافی نیست. موجودی: {product.StockQuantity}";
                return response;
            }

            // Get or create basket
            var basket = await _basketRepository.GetActiveBasketByUserIdAsync(dto.UserId);
            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = dto.UserId,
                    Status = BasketStatus.Active,
                    Items = new List<BasketItem>()
                };
                basket = await _basketRepository.CreateAsync(basket);
            }

            // Add item
            var existingItem = basket.Items.FirstOrDefault(x => x.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                await _basketRepository.UpdateBasketItemAsync(existingItem);
            }
            else
            {
                var basketItem = new BasketItem
                {
                    BasketId = basket.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price
                };
                await _basketRepository.AddBasketItemAsync(basketItem);
            }

            response.Success = true;
            response.Message = "محصول با موفقیت به سبد خرید اضافه شد";
            response.Basket = await GetBasketByUserIdAsync(dto.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to basket for user {UserId}", dto.ProductId, dto.UserId);
            response.Success = false;
            response.Message = "خطا در افزودن محصول به سبد خرید";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> UpdateBasketItemAsync(UpdateBasketItemDto dto)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var basketItem = await _basketRepository.GetBasketItemByIdAsync(dto.BasketItemId);

            if (basketItem == null)
            {
                response.Success = false;
                response.Message = "آیتم مورد نظر یافت نشد";
                return response;
            }

            if (dto.Quantity <= 0)
            {
                await _basketRepository.RemoveBasketItemAsync(dto.BasketItemId);
                response.Message = "محصول از سبد خرید حذف شد";
            }
            else
            {
                // Check stock
                var product = await _productRepository.GetByIdAsync(basketItem.ProductId);
                if (product != null && product.StockQuantity < dto.Quantity)
                {
                    response.Success = false;
                    response.Message = $"موجودی کافی نیست. حداکثر تعداد مجاز: {product.StockQuantity}";
                    return response;
                }

                basketItem.Quantity = dto.Quantity;
                await _basketRepository.UpdateBasketItemAsync(basketItem);
                response.Message = "تعداد محصول با موفقیت بروزرسانی شد";
            }

            response.Success = true;
            var basket = await _basketRepository.GetBasketWithItemsAsync(basketItem.BasketId);
            if (basket != null && basket.UserId.HasValue)
            {
                response.Basket = await GetBasketByUserIdAsync(basket.UserId.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket item {BasketItemId}", dto.BasketItemId);
            response.Success = false;
            response.Message = "خطا در بروزرسانی سبد خرید";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> RemoveFromBasketAsync(int basketItemId)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var result = await _basketRepository.RemoveBasketItemAsync(basketItemId);

            response.Success = result;
            response.Message = result ? "محصول با موفقیت حذف شد" : "حذف انجام نشد";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing basket item {BasketItemId}", basketItemId);
            response.Success = false;
            response.Message = "خطا در حذف محصول";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> ClearBasketAsync(int userId)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var basket = await _basketRepository.GetActiveBasketByUserIdAsync(userId);

            if (basket == null)
            {
                response.Success = false;
                response.Message = "سبد خرید یافت نشد";
                return response;
            }

            await _basketRepository.ClearBasketAsync(basket.Id);

            response.Success = true;
            response.Message = "سبد خرید با موفقیت خالی شد";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing basket for user {UserId}", userId);
            response.Success = false;
            response.Message = "خطا در خالی کردن سبد خرید";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> ApplyDiscountAsync(ApplyDiscountDto dto)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var basket = await _basketRepository.GetBasketWithItemsAsync(dto.BasketId);

            if (basket == null)
            {
                response.Success = false;
                response.Message = "سبد خرید یافت نشد";
                return response;
            }

            // Discount logic (می‌توانید از دیتابیس بخوانید)
            var discountRules = new Dictionary<string, (int Percentage, decimal MaxAmount)>
            {
                { "WELCOME10", (10, 500000) },
                { "SUMMER20", (20, 1000000) },
                { "SAVE15", (15, 750000) }
            };

            if (discountRules.TryGetValue(dto.DiscountCode.ToUpper(), out var rule))
            {
                var discountAmount = basket.SubTotal * rule.Percentage / 100;
                if (discountAmount > rule.MaxAmount)
                    discountAmount = rule.MaxAmount;

                basket.ApplyDiscount(dto.DiscountCode.ToUpper(), discountAmount, rule.Percentage);
                await _basketRepository.UpdateAsync(basket);

                response.Success = true;
                response.Message = $"کد تخفیف با {rule.Percentage}% اعمال شد";
                if (basket.UserId.HasValue)
                    response.Basket = await GetBasketByUserIdAsync(basket.UserId.Value);
            }
            else
            {
                response.Success = false;
                response.Message = "کد تخفیف نامعتبر است";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying discount {DiscountCode}", dto.DiscountCode);
            response.Success = false;
            response.Message = "خطا در اعمال کد تخفیف";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> RemoveDiscountAsync(int basketId)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var basket = await _basketRepository.GetBasketWithItemsAsync(basketId);

            if (basket == null)
            {
                response.Success = false;
                response.Message = "سبد خرید یافت نشد";
                return response;
            }

            basket.RemoveDiscount();
            await _basketRepository.UpdateAsync(basket);

            response.Success = true;
            response.Message = "کد تخفیف با موفقیت حذف شد";
            if (basket.UserId.HasValue)
                response.Basket = await GetBasketByUserIdAsync(basket.UserId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing discount from basket {BasketId}", basketId);
            response.Success = false;
            response.Message = "خطا در حذف کد تخفیف";
        }

        return response;
    }

    public async Task<BasketOperationResponseDto> MergeBasketAsync(MergeBasketDto dto)
    {
        var response = new BasketOperationResponseDto();

        try
        {
            var anonymousBasket = await _basketRepository.GetActiveBasketByUserIdAsync(dto.AnonymousUserId);
            var registeredBasket = await _basketRepository.GetActiveBasketByUserIdAsync(dto.RegisteredUserId);

            if (registeredBasket == null)
            {
                registeredBasket = new Basket
                {
                    UserId = dto.RegisteredUserId,
                    Status = BasketStatus.Active,
                    Items = new List<BasketItem>()
                };
                registeredBasket = await _basketRepository.CreateAsync(registeredBasket);
            }

            if (anonymousBasket != null && anonymousBasket.Items.Any())
            {
                foreach (var item in anonymousBasket.Items)
                {
                    var existingItem = registeredBasket.Items.FirstOrDefault(x => x.ProductId == item.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += item.Quantity;
                        await _basketRepository.UpdateBasketItemAsync(existingItem);
                    }
                    else
                    {
                        item.BasketId = registeredBasket.Id;
                        item.Id = 0;
                        await _basketRepository.AddBasketItemAsync(item);
                    }
                }

                // Mark anonymous basket as converted
                anonymousBasket.Status = BasketStatus.ConvertedToOrder;
                await _basketRepository.UpdateAsync(anonymousBasket);
            }

            response.Success = true;
            response.Message = "سبد خرید با موفقیت ادغام شد";
            response.Basket = await GetBasketByUserIdAsync(dto.RegisteredUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging baskets");
            response.Success = false;
            response.Message = "خطا در ادغام سبد خرید";
        }

        return response;
    }

    public async Task<BasketDto> PrepareForCheckoutAsync(int basketId)
    {
        try
        {
            var basket = await _basketRepository.GetBasketWithItemsAsync(basketId);

            if (basket == null)
                throw new NotFoundException($"سبد خرید با شناسه {basketId} یافت نشد");

            if (!basket.Items.Any())
                throw new BusinessRuleException("سبد خرید خالی است");

            // Check stock for all items
            foreach (var item in basket.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new NotFoundException($"محصول با شناسه {item.ProductId} یافت نشد");

                if (product.StockQuantity < item.Quantity)
                    throw new BusinessRuleException($"موجودی محصول '{product.Name}' کافی نیست");
            }

            basket.Status = BasketStatus.ReadyForCheckout;
            basket.UpdatedAt = DateTime.UtcNow;
            await _basketRepository.UpdateAsync(basket);

            return MapToBasketDto(basket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing basket {BasketId} for checkout", basketId);
            throw;
        }
    }

    #region Private Methods

    private BasketDto MapToBasketDto(Basket basket)
    {
        if (basket == null) return null!;

        return new BasketDto
        {
            Id = basket.Id,
            UserId = basket.UserId,
            SessionId = basket.SessionId,
            Status = basket.Status.ToString(),
            DiscountCode = basket.DiscountCode,
            DiscountAmount = basket.DiscountAmount,
            SubTotal = basket.SubTotal,
            TotalDiscount = basket.TotalDiscount,
            TotalPrice = basket.TotalPrice,
            TotalItems = basket.TotalItems,
            Items = basket.Items.Select(item => new BasketItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductImage = item.Product?.MainImage,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            }).ToList(),
            CreatedAt = basket.CreatedAt,
            UpdatedAt = basket.UpdatedAt
        };
    }

    #endregion
}