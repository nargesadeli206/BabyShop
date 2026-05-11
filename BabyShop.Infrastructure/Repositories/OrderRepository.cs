using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // ============ متدهای پایه ============

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE IsDeleted = 0 ORDER BY CreatedAt DESC"
        );

        return result.AsList();
    }

    public async Task<Order> AddAsync(Order entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", entity.UserId);
        parameters.Add("@Status", entity.Status);
        parameters.Add("@SubTotal", entity.SubTotal);
        parameters.Add("@Discount", entity.Discount);
        parameters.Add("@Tax", entity.Tax);
        parameters.Add("@ShippingCost", entity.ShippingCost);
        parameters.Add("@TotalAmount", entity.TotalAmount);
        parameters.Add("@ShippingAddress", entity.ShippingAddress);
        parameters.Add("@PhoneNumber", entity.PhoneNumber);
        parameters.Add("@Email", entity.Email);
        parameters.Add("@CustomerNote", entity.CustomerNote);
        parameters.Add("@OrderNumber", entity.OrderNumber);
        parameters.Add("@IsReminderSent", 0);
        parameters.Add("@IsArchived", 0);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            @"INSERT INTO Orders (UserId, Status, SubTotal, Discount, Tax, ShippingCost, TotalAmount, 
              ShippingAddress, PhoneNumber, Email, CustomerNote, OrderNumber, IsDeleted, IsReminderSent, IsArchived, CreatedAt) 
              VALUES (@UserId, @Status, @SubTotal, @Discount, @Tax, @ShippingCost, @TotalAmount, 
              @ShippingAddress, @PhoneNumber, @Email, @CustomerNote, @OrderNumber, 0, @IsReminderSent, @IsArchived, GETDATE());
              SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Order entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            @"UPDATE Orders SET 
                Status = @Status,
                SubTotal = @SubTotal,
                Discount = @Discount,
                Tax = @Tax,
                ShippingCost = @ShippingCost,
                TotalAmount = @TotalAmount,
                ShippingAddress = @ShippingAddress,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                CustomerNote = @CustomerNote,
                AdminNote = @AdminNote,
                PaidAt = @PaidAt,
                ShippedAt = @ShippedAt,
                DeliveredAt = @DeliveredAt,
                CancellationReason = @CancellationReason,
                IsArchived = @IsArchived,
                ArchivedAt = @ArchivedAt,
                IsReminderSent = @IsReminderSent,
                ReminderSentAt = @ReminderSentAt,
                UpdatedAt = GETDATE()
              WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                Status = entity.Status,
                SubTotal = entity.SubTotal,
                Discount = entity.Discount,
                Tax = entity.Tax,
                ShippingCost = entity.ShippingCost,
                TotalAmount = entity.TotalAmount,
                ShippingAddress = entity.ShippingAddress,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                CustomerNote = entity.CustomerNote,
                AdminNote = entity.AdminNote,
                PaidAt = entity.PaidAt,
                ShippedAt = entity.ShippedAt,
                DeliveredAt = entity.DeliveredAt,
                CancellationReason = entity.CancellationReason,
                IsArchived = entity.IsArchived,
                ArchivedAt = entity.ArchivedAt,
                IsReminderSent = entity.IsReminderSent,
                ReminderSentAt = entity.ReminderSentAt
            }
        );
    }

    public async Task DeleteAsync(Order entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Orders SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var order = await GetByIdAsync(id);
        return order != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Orders WHERE IsDeleted = 0"
        );
    }

    // ============ متدهای اختصاصی Order ============

    public async Task<Order?> GetOrderWithItemsAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        var order = await connection.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );

        if (order == null) return null;

        var items = await connection.QueryAsync<OrderItem>(
            "SELECT * FROM OrderItems WHERE OrderId = @OrderId",
            new { OrderId = id }
        );

        foreach (var item in items)
        {
            order.AddItem(item);
        }

        return order;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE UserId = @UserId AND IsDeleted = 0 ORDER BY CreatedAt DESC",
            new { UserId = userId }
        );

        return result.AsList();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE Status = @Status AND IsDeleted = 0 ORDER BY CreatedAt DESC",
            new { Status = status }
        );

        return result.AsList();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate AND IsDeleted = 0 ORDER BY CreatedAt DESC",
            new { StartDate = startDate, EndDate = endDate }
        );

        return result.AsList();
    }

    public async Task<List<Order>> GetOldDeliveredOrdersAsync(DateTime olderThan)
    {
        using var connection = new SqlConnection(_connectionString);

        var orders = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE IsDeleted = 0 AND Status = 'Delivered' AND CreatedAt < @OlderThan",
            new { OlderThan = olderThan }
        );

        var result = orders.ToList();

        foreach (var order in result)
        {
            var items = await connection.QueryAsync<OrderItem>(
                "SELECT * FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = order.Id }
            );

            foreach (var item in items)
            {
                order.AddItem(item);
            }
        }

        return result;
    }

    public async Task<List<Order>> GetAbandonedCartsAsync(DateTime olderThan)
    {
        using var connection = new SqlConnection(_connectionString);

        var orders = await connection.QueryAsync<Order>(
            @"SELECT o.*
              FROM Orders o
              WHERE o.IsDeleted = 0 
                AND o.Status = 'Pending'
                AND o.IsPaid = 0
                AND o.IsReminderSent = 0
                AND o.CreatedAt < @OlderThan",
            new { OlderThan = olderThan }
        );

        var result = orders.ToList();

        foreach (var order in result)
        {
            var items = await connection.QueryAsync<OrderItem>(
                "SELECT * FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = order.Id }
            );

            foreach (var item in items)
            {
                order.AddItem(item);
            }
        }

        return result;
    }

    // ============ متد کمکی برای جستجو ============

    public async Task<IReadOnlyList<Order>> FindAsync(Func<Order, bool> predicate)
    {
        var all = await GetAllAsync();
        var result = all.Where(predicate).ToList();
        return result.AsReadOnly();
    }
}