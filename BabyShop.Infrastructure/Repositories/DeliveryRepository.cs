using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly string _connectionString;

    public DeliveryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // ============ متدهای پایه ============

    public async Task<Delivery?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Delivery>(
            "SELECT * FROM Deliveries WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Delivery>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Delivery>(
            "SELECT * FROM Deliveries WHERE IsDeleted = 0 ORDER BY CreatedAt DESC"
        );

        return result.AsList();
    }

    public async Task<Delivery> AddAsync(Delivery entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@OrderId", entity.OrderId);
        parameters.Add("@Address", entity.Address);
        parameters.Add("@PhoneNumber", entity.PhoneNumber);
        parameters.Add("@PostalCode", entity.PostalCode);
        parameters.Add("@Carrier", entity.Carrier);
        parameters.Add("@EstimatedDeliveryDate", entity.EstimatedDeliveryDate);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Deliveries (OrderId, Address, PhoneNumber, PostalCode, Status, Carrier, EstimatedDeliveryDate, IsDeleted, CreatedAt) " +
            "VALUES (@OrderId, @Address, @PhoneNumber, @PostalCode, 'Pending', @Carrier, @EstimatedDeliveryDate, 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Delivery entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Deliveries SET " +
            "Status = @Status, " +
            "TrackingNumber = @TrackingNumber, " +
            "ActualDeliveryDate = @ActualDeliveryDate, " +
            "UpdatedAt = GETDATE() " +
            "WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                Status = entity.Status,
                TrackingNumber = entity.TrackingNumber,
                ActualDeliveryDate = entity.ActualDeliveryDate
            }
        );
    }

    public async Task DeleteAsync(Delivery entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Deliveries SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var delivery = await GetByIdAsync(id);
        return delivery != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Deliveries WHERE IsDeleted = 0"
        );
    }

    // ============ متدهای اختصاصی Delivery ============

    public async Task<Delivery?> GetByOrderIdAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Delivery>(
            "SELECT * FROM Deliveries WHERE OrderId = @OrderId AND IsDeleted = 0",
            new { OrderId = orderId }
        );
    }

    public async Task<List<Delivery>> GetPendingDeliveriesAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Delivery>(
            "SELECT * FROM Deliveries WHERE (Status = 'Pending' OR Status = 'Processing') AND IsDeleted = 0"
        );

        return result.ToList();
    }

    public async Task<Delivery> CreateDeliveryAsync(Delivery delivery)
    {
        return await AddAsync(delivery);
    }

    public async Task<Delivery> UpdateDeliveryStatusAsync(int deliveryId, string status, string? trackingNumber)
    {
        var delivery = await GetByIdAsync(deliveryId);
        if (delivery == null)
            return null!;

        // بروزرسانی فیلدها با استفاده از متدهای Business Logic
        switch (status)
        {
            case "Processing":
                delivery.MarkAsProcessing();
                break;
            case "Shipped":
                delivery.MarkAsShipped(trackingNumber ?? string.Empty);
                break;
            case "Delivered":
                delivery.MarkAsDelivered();
                break;
            case "Failed":
                delivery.MarkAsFailed("Delivery failed");
                break;
            default:
                return delivery;
        }

        await UpdateAsync(delivery);
        return delivery;
    }

    public async Task<bool> IsOrderPaidAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.ExecuteScalarAsync<int>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM Orders WHERE Id = @OrderId AND Status = 'Paid' AND IsDeleted = 0) THEN 1 ELSE 0 END",
            new { OrderId = orderId }
        );

        return result == 1;
    }
}