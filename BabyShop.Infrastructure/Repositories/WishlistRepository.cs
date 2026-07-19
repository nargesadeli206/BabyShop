using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly string _connectionString;

    public WishlistRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IReadOnlyList<Wishlist>> GetWishlistByUserIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Wishlist, Product, Wishlist>(
            @"SELECT w.*, p.*
              FROM Wishlists w
              INNER JOIN Products p ON w.ProductId = p.Id
              WHERE w.UserId = @UserId
              ORDER BY w.CreatedAt DESC",
            (wishlist, product) =>
            {
                wishlist.Product = product;
                return wishlist;
            },
            new { UserId = userId },
            splitOn: "Id"
        );

        return result.ToList().AsReadOnly();
    }

    public async Task<Wishlist?> GetWishlistItemAsync(int userId, int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Wishlist>(
            "SELECT * FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId",
            new { UserId = userId, ProductId = productId }
        );
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId",
            new { UserId = userId, ProductId = productId }
        );

        return count > 0;
    }

    public async Task<Wishlist> AddToWishlistAsync(int userId, int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@ProductId", productId);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Wishlists (UserId, ProductId, CreatedAt) VALUES (@UserId, @ProductId, GETDATE()); SET @Id = SCOPE_IDENTITY();",
            parameters);

        var wishlist = new Wishlist(userId, productId);
        wishlist.Id = parameters.Get<int>("@Id");

        return wishlist;
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        var rows = await connection.ExecuteAsync(
            "DELETE FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId",
            new { UserId = userId, ProductId = productId }
        );

        return rows > 0;
    }

    public async Task<int> GetWishlistCountAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Wishlists WHERE UserId = @UserId",
            new { UserId = userId }
        );
    }

    public async Task ClearWishlistAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "DELETE FROM Wishlists WHERE UserId = @UserId",
            new { UserId = userId }
        );
    }
}