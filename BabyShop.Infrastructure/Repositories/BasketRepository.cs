using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class BasketRepository : IBasketRepository
{
    private readonly string _connectionString;
    private readonly IProductRepository _productRepository;

    public BasketRepository(IConfiguration configuration, IProductRepository productRepository)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _productRepository = productRepository;
    }

    // ============ متدهای پایه ============

    public async Task<Basket?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Basket>(
            "SELECT * FROM Baskets WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Basket>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Basket>(
            "SELECT * FROM Baskets WHERE IsDeleted = 0 ORDER BY CreatedAt DESC"
        );

        return result.AsList();
    }

    public async Task<Basket> AddAsync(Basket entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", entity.UserId);
        parameters.Add("@Status", entity.Status);
        parameters.Add("@TotalPrice", entity.TotalPrice);
        parameters.Add("@DiscountCode", entity.DiscountCode);
        parameters.Add("@DiscountAmount", entity.DiscountAmount);
        parameters.Add("@DiscountPercentage", entity.DiscountPercentage);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Baskets (UserId, Status, TotalPrice, DiscountCode, DiscountAmount, DiscountPercentage, IsDeleted, CreatedAt) " +
            "VALUES (@UserId, @Status, @TotalPrice, @DiscountCode, @DiscountAmount, @DiscountPercentage, 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Basket entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Baskets SET " +
            "Status = @Status, " +
            "TotalPrice = @TotalPrice, " +
            "DiscountCode = @DiscountCode, " +
            "DiscountAmount = @DiscountAmount, " +
            "DiscountPercentage = @DiscountPercentage, " +
            "UpdatedAt = GETDATE() " +
            "WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                Status = entity.Status,
                TotalPrice = entity.TotalPrice,
                DiscountCode = entity.DiscountCode,
                DiscountAmount = entity.DiscountAmount,
                DiscountPercentage = entity.DiscountPercentage
            }
        );
    }

    public async Task DeleteAsync(Basket entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Baskets SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var basket = await GetByIdAsync(id);
        return basket != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Baskets WHERE IsDeleted = 0"
        );
    }

    // ============ متدهای اختصاصی Basket ============

    public async Task<Basket?> GetActiveBasketByUserIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        var basketDict = new Dictionary<int, Basket>();

        var result = await connection.QueryAsync<Basket, BasketItem, Product, Basket>(
            @"SELECT b.*, bi.*, p.*
              FROM Baskets b
              LEFT JOIN BasketItems bi ON b.Id = bi.BasketId
              LEFT JOIN Products p ON bi.ProductId = p.Id
              WHERE b.UserId = @UserId AND b.Status = 'Active' AND b.IsDeleted = 0",
            (basket, item, product) =>
            {
                if (!basketDict.TryGetValue(basket.Id, out var currentBasket))
                {
                    currentBasket = basket;
                    currentBasket.Items = new List<BasketItem>();
                    basketDict.Add(basket.Id, currentBasket);
                }

                if (item != null)
                {
                    item.Product = product;
                    currentBasket.Items.Add(item);
                }

                return currentBasket;
            },
            new { UserId = userId },
            splitOn: "Id,Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<Basket?> GetBasketWithItemsAsync(int basketId)
    {
        using var connection = new SqlConnection(_connectionString);

        var basketDict = new Dictionary<int, Basket>();

        var result = await connection.QueryAsync<Basket, BasketItem, Product, Basket>(
            @"SELECT b.*, bi.*, p.*
              FROM Baskets b
              LEFT JOIN BasketItems bi ON b.Id = bi.BasketId
              LEFT JOIN Products p ON bi.ProductId = p.Id
              WHERE b.Id = @BasketId AND b.IsDeleted = 0",
            (basket, item, product) =>
            {
                if (!basketDict.TryGetValue(basket.Id, out var currentBasket))
                {
                    currentBasket = basket;
                    currentBasket.Items = new List<BasketItem>();
                    basketDict.Add(basket.Id, currentBasket);
                }

                if (item != null)
                {
                    item.Product = product;
                    currentBasket.Items.Add(item);
                }

                return currentBasket;
            },
            new { BasketId = basketId },
            splitOn: "Id,Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<Basket?> GetOrCreateActiveBasketAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "GetOrCreateActiveBasket",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        var basketId = parameters.Get<int>("@Id");
        return await GetBasketWithItemsAsync(basketId);
    }

    public async Task<BasketItem?> GetBasketItemByProductAsync(int basketId, int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<BasketItem>(
            "SELECT * FROM BasketItems WHERE BasketId = @BasketId AND ProductId = @ProductId",
            new { BasketId = basketId, ProductId = productId }
        );
    }

    public async Task<BasketItem?> GetBasketItemByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<BasketItem>(
            "SELECT * FROM BasketItems WHERE Id = @Id",
            new { Id = id }
        );
    }

    public async Task AddBasketItemAsync(BasketItem item)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@BasketId", item.BasketId);
        parameters.Add("@ProductId", item.ProductId);
        parameters.Add("@Quantity", item.Quantity);
        parameters.Add("@UnitPrice", item.UnitPrice);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO BasketItems (BasketId, ProductId, Quantity, UnitPrice, IsDeleted, CreatedAt) " +
            "VALUES (@BasketId, @ProductId, @Quantity, @UnitPrice, 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        item.Id = parameters.Get<int>("@Id");
    }

    public async Task UpdateBasketItemAsync(BasketItem item)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE BasketItems SET Quantity = @Quantity, UpdatedAt = GETDATE() WHERE Id = @Id",
            new { Id = item.Id, Quantity = item.Quantity }
        );
    }

    public async Task<bool> RemoveBasketItemAsync(int basketItemId)
    {
        using var connection = new SqlConnection(_connectionString);

        var rows = await connection.ExecuteAsync(
            "DELETE FROM BasketItems WHERE Id = @Id",
            new { Id = basketItemId }
        );

        return rows > 0;
    }

    public async Task<bool> ClearBasketAsync(int basketId)
    {
        using var connection = new SqlConnection(_connectionString);

        var rows = await connection.ExecuteAsync(
            "DELETE FROM BasketItems WHERE BasketId = @BasketId",
            new { BasketId = basketId }
        );

        return rows > 0;
    }

    public async Task<int> GetBasketItemsCountAsync(int userId)
    {
        var basket = await GetActiveBasketByUserIdAsync(userId);
        return basket?.Items?.Count ?? 0;
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _productRepository.ExistsAsync(productId);
    }

    public async Task<Basket> AddToBasketAsync(int userId, int productId, int quantity)
    {
        var basket = await GetOrCreateActiveBasketAsync(userId);
        if (basket == null)
            throw new Exception("Could not create or retrieve basket");

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new Exception("Product not found");

        var existingItem = await GetBasketItemByProductAsync(basket.Id, productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            await UpdateBasketItemAsync(existingItem);
        }
        else
        {
            var newItem = new BasketItem
            {
                BasketId = basket.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };
            await AddBasketItemAsync(newItem);
        }

        await UpdateBasketTotalPrice(basket.Id);
        return await GetBasketWithItemsAsync(basket.Id) ?? basket;
    }

    public async Task<Basket> UpdateBasketItemAsync(int basketItemId, int quantity)
    {
        var item = await GetBasketItemByIdAsync(basketItemId);
        if (item == null)
            throw new Exception("Basket item not found");

        if (quantity <= 0)
        {
            await RemoveBasketItemAsync(basketItemId);
        }
        else
        {
            item.Quantity = quantity;
            await UpdateBasketItemAsync(item);
        }

        await UpdateBasketTotalPrice(item.BasketId);
        return await GetBasketWithItemsAsync(item.BasketId) ?? throw new Exception("Basket not found");
    }

    public async Task<Basket> ApplyDiscountAsync(int basketId, string discountCode)
    {
        var basket = await GetBasketWithItemsAsync(basketId);
        if (basket == null)
            throw new Exception("Basket not found");

        decimal discountPercentage = discountCode == "WELCOME10" ? 0.10m : 0;
        var discountAmount = basket.SubTotal * discountPercentage;

        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Baskets SET DiscountCode = @DiscountCode, DiscountAmount = @DiscountAmount, DiscountPercentage = @DiscountPercentage, UpdatedAt = GETDATE() " +
            "WHERE Id = @BasketId AND IsDeleted = 0",
            new { BasketId = basketId, DiscountCode = discountCode, DiscountAmount = discountAmount, DiscountPercentage = discountPercentage * 100 }
        );

        await UpdateBasketTotalPrice(basketId);
        return await GetBasketWithItemsAsync(basketId) ?? basket;
    }

    public async Task<bool> RemoveDiscountAsync(int basketId)
    {
        using var connection = new SqlConnection(_connectionString);

        var rows = await connection.ExecuteAsync(
            "UPDATE Baskets SET DiscountCode = NULL, DiscountAmount = NULL, DiscountPercentage = NULL, UpdatedAt = GETDATE() " +
            "WHERE Id = @BasketId AND IsDeleted = 0",
            new { BasketId = basketId }
        );

        await UpdateBasketTotalPrice(basketId);
        return rows > 0;
    }

    private async Task UpdateBasketTotalPrice(int basketId)
    {
        var basket = await GetBasketWithItemsAsync(basketId);
        if (basket != null && basket.Items != null)
        {
            var totalPrice = basket.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (basket.DiscountAmount.HasValue)
            {
                totalPrice -= basket.DiscountAmount.Value;
            }

            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "UPDATE Baskets SET TotalPrice = @TotalPrice, UpdatedAt = GETDATE() WHERE Id = @Id AND IsDeleted = 0",
                new { Id = basketId, TotalPrice = totalPrice }
            );
        }
    }
}