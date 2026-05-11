using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly string _connectionString;

    public InventoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // ============ متدهای پایه ============

    public async Task<Inventory?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Inventory>(
            "SELECT * FROM Inventories WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Inventory>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Inventory>(
            "SELECT * FROM Inventories WHERE IsDeleted = 0"
        );

        return result.AsList();
    }

    public async Task<Inventory> AddAsync(Inventory entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@ProductId", entity.ProductId);
        parameters.Add("@CurrentStock", entity.CurrentStock);
        parameters.Add("@ReservedStock", entity.ReservedStock);
        parameters.Add("@ReorderPoint", entity.ReorderPoint);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Inventories (ProductId, CurrentStock, ReservedStock, ReorderPoint, IsDeleted, CreatedAt) " +
            "VALUES (@ProductId, @CurrentStock, @ReservedStock, @ReorderPoint, 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Inventory entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Inventories SET " +
            "ProductId = @ProductId, " +
            "CurrentStock = @CurrentStock, " +
            "ReservedStock = @ReservedStock, " +
            "ReorderPoint = @ReorderPoint, " +
            "UpdatedAt = GETDATE() " +
            "WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                CurrentStock = entity.CurrentStock,
                ReservedStock = entity.ReservedStock,
                ReorderPoint = entity.ReorderPoint
            }
        );
    }

    public async Task DeleteAsync(Inventory entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Inventories SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var inventory = await GetByIdAsync(id);
        return inventory != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Inventories WHERE IsDeleted = 0"
        );
    }

    // ============ متدهای اختصاصی Inventory ============

    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Inventory, Product, Inventory>(
            @"SELECT i.*, p.*
              FROM Inventories i
              LEFT JOIN Products p ON i.ProductId = p.Id
              WHERE i.ProductId = @ProductId AND i.IsDeleted = 0",
            (inventory, product) =>
            {
                inventory.Product = product;
                return inventory;
            },
            new { ProductId = productId },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Inventory>> GetAllWithProductAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Inventory, Product, Inventory>(
            @"SELECT i.*, p.*
              FROM Inventories i
              LEFT JOIN Products p ON i.ProductId = p.Id
              WHERE i.IsDeleted = 0",
            (inventory, product) =>
            {
                inventory.Product = product;
                return inventory;
            },
            splitOn: "Id"
        );

        return result.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockInventoriesAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Inventory, Product, Inventory>(
            @"SELECT i.*, p.*
              FROM Inventories i
              LEFT JOIN Products p ON i.ProductId = p.Id
              WHERE i.IsDeleted = 0 AND i.CurrentStock <= i.ReorderPoint",
            (inventory, product) =>
            {
                inventory.Product = product;
                return inventory;
            },
            splitOn: "Id"
        );

        return result.ToList().AsReadOnly();
    }

    public async Task DecreaseStockAsync(int productId, int quantity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Inventories SET CurrentStock = CurrentStock - @Quantity, UpdatedAt = GETDATE() " +
            "WHERE ProductId = @ProductId AND IsDeleted = 0",
            new { ProductId = productId, Quantity = quantity }
        );
    }

    public async Task IncreaseStockAsync(int productId, int quantity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Inventories SET CurrentStock = CurrentStock + @Quantity, UpdatedAt = GETDATE() " +
            "WHERE ProductId = @ProductId AND IsDeleted = 0",
            new { ProductId = productId, Quantity = quantity }
        );
    }
}