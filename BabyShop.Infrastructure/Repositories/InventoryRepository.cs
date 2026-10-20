using BabyShop.Core.Entities;
using BabyShop.Core.Entities.Base;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BabyShop.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is missing.");
        }

        public async Task<Inventory?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Inventory>(
                "SELECT * FROM Inventories WHERE Id = @Id AND IsDeleted = 0", new { Id = id });
        }

        public async Task<Inventory> AddAsync(Inventory entity)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"INSERT INTO Inventories (ProductId, AgeRange, CurrentStock, ReservedStock, ReorderPoint, IsDeleted, CreatedAt) 
                        VALUES (@ProductId, @AgeRange, @CurrentStock, @ReservedStock, @ReorderPoint, 0, GETDATE()); 
                        SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<int>(sql, entity);
            typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }

        public async Task UpdateAsync(Inventory entity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Inventories SET CurrentStock = @CurrentStock, ReservedStock = @ReservedStock, AgeRange = @AgeRange, UpdatedAt = GETDATE() WHERE Id = @Id",
                entity);
        }

        public async Task DeleteAsync(Inventory entity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Inventories SET IsDeleted = 1, DeletedAt = GETDATE() WHERE Id = @Id", new { Id = entity.Id });
        }

        public async Task<IReadOnlyList<Inventory>> GetAllAsync()
        {
            return await GetAllWithProductAsync();
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Inventories WHERE IsDeleted = 0");
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return (await GetByIdAsync(id)) != null;
        }

        // ================= متدهای اختصاصی IInventoryRepository ================= //

        public async Task<Inventory?> GetByProductIdAsync(int productId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT i.*, p.* FROM Inventories i 
                        LEFT JOIN Products p ON i.ProductId = p.Id 
                        WHERE i.ProductId = @ProductId AND i.IsDeleted = 0";

            var result = await connection.QueryAsync<Inventory, Product, Inventory>(
                sql, (inv, prod) => { inv.Product = prod; return inv; },
                new { ProductId = productId }, splitOn: "Id");

            return result.FirstOrDefault();
        }

        public async Task<List<Inventory>> GetAllInventoriesByProductIdAsync(int productId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT * FROM Inventories WHERE ProductId = @ProductId AND IsDeleted = 0";
            var result = await connection.QueryAsync<Inventory>(sql, new { ProductId = productId });
            return result.ToList();
        }

        public async Task<Inventory?> GetProductInventoryByAgeRangeAsync(int productId, string ageRange)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"SELECT * FROM Inventories WHERE ProductId = @ProductId AND AgeRange = @AgeRange AND IsDeleted = 0";
            return await connection.QueryFirstOrDefaultAsync<Inventory>(sql, new { ProductId = productId, AgeRange = ageRange });
        }

        public async Task IncreaseStockAsync(int productId, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Inventories SET CurrentStock = CurrentStock + @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId",
                new { ProductId = productId, Quantity = quantity });
        }

        public async Task DecreaseStockAsync(int productId, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Inventories SET CurrentStock = CurrentStock - @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId",
                new { ProductId = productId, Quantity = quantity });
        }

        public async Task IncreaseStockAsync(int productId, string ageRange, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Inventories SET CurrentStock = CurrentStock + @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId AND AgeRange = @AgeRange",
                new { ProductId = productId, AgeRange = ageRange, Quantity = quantity });
        }

        public async Task DecreaseStockAsync(int productId, string ageRange, int quantity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Inventories SET CurrentStock = CurrentStock - @Quantity, UpdatedAt = GETDATE() WHERE ProductId = @ProductId AND AgeRange = @AgeRange",
                new { ProductId = productId, AgeRange = ageRange, Quantity = quantity });
        }

        public async Task<IReadOnlyList<Inventory>> GetAllWithProductAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT i.*, p.* FROM Inventories i INNER JOIN Products p ON i.ProductId = p.Id WHERE i.IsDeleted = 0";
            var result = await connection.QueryAsync<Inventory, Product, Inventory>(
                sql, (inv, prod) => { inv.Product = prod; return inv; }, splitOn: "Id");
            return result.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Inventory>> GetLowStockInventoriesAsync()
        {
            var all = await GetAllWithProductAsync();
            return all.Where(x => x.IsLowStock).ToList().AsReadOnly();
        }

        // ================= متدهای گمشده IRepository (برای رفع ارورها) ================= //

        public async Task<IReadOnlyList<Inventory>> GetPagedAsync(int pageNumber, int pageSize)
        {
            using var connection = new SqlConnection(_connectionString);
            var offset = (pageNumber - 1) * pageSize;
            var sql = @"SELECT * FROM Inventories WHERE IsDeleted = 0 
                        ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            var result = await connection.QueryAsync<Inventory>(sql, new { Offset = offset, PageSize = pageSize });
            return result.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<Inventory>> FindAsync(Expression<Func<Inventory, bool>> predicate)
        {
            // در Dapper استفاده از Expression سخته، بنابراین اول همه رو می‌گیریم و در حافظه فیلتر می‌کنیم
            var all = await GetAllAsync();
            return all.Where(predicate.Compile()).ToList().AsReadOnly();
        }

        public async Task<Inventory?> FirstOrDefaultAsync(Expression<Func<Inventory, bool>> predicate)
        {
            var all = await GetAllAsync();
            return all.FirstOrDefault(predicate.Compile());
        }
    }
}