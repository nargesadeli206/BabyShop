using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BabyShop.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;

    public PaymentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // ============ متدهای پایه ============

    public async Task<Payment?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Payment>(
            "SELECT * FROM Payments WHERE IsDeleted = 0 ORDER BY CreatedAt DESC"
        );

        return result.AsList();
    }

    public async Task<Payment> AddAsync(Payment entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@OrderId", entity.OrderId);
        parameters.Add("@Amount", entity.Amount);
        parameters.Add("@PaymentMethod", entity.PaymentMethod);
        parameters.Add("@Authority", entity.Authority);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Payments (OrderId, Amount, PaymentMethod, Authority, Status, IsDeleted, CreatedAt) " +
            "VALUES (@OrderId, @Amount, @PaymentMethod, @Authority, 'Pending', 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Payment entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Payments SET " +
            "Status = @Status, " +
            "ReferenceNumber = @ReferenceNumber, " +
            "PaidAt = @PaidAt, " +
            "RefundedAt = @RefundedAt, " +
            "RefundReason = @RefundReason, " +
            "UpdatedAt = GETDATE() " +
            "WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                Status = entity.Status,
                ReferenceNumber = entity.ReferenceNumber,
                PaidAt = entity.PaidAt,
                RefundedAt = entity.RefundedAt,
                RefundReason = entity.RefundReason
            }
        );
    }

    public async Task DeleteAsync(Payment entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Payments SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var payment = await GetByIdAsync(id);
        return payment != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Payments WHERE IsDeleted = 0"
        );
    }

    // ============ متدهای اختصاصی Payment ============

    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Payment, Order, Payment>(
            @"SELECT p.*, o.*
              FROM Payments p
              LEFT JOIN Orders o ON p.OrderId = o.Id
              WHERE p.OrderId = @OrderId AND p.IsDeleted = 0",
            (payment, order) =>
            {
                payment.Order = order;
                return payment;
            },
            new { OrderId = orderId },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<Payment?> GetPaymentByAuthorityAsync(string authority)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE Authority = @Authority AND IsDeleted = 0",
            new { Authority = authority }
        );
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        if (string.IsNullOrEmpty(payment.Authority))
        {
            payment.SetAuthority(Guid.NewGuid().ToString());
        }

        return await AddAsync(payment);
    }

    public async Task<Payment> UpdatePaymentAsync(Payment payment)
    {
        await UpdateAsync(payment);
        return payment;
    }

    public async Task<Payment> VerifyPaymentAsync(int paymentId, string referenceNumber)
    {
        var payment = await GetByIdAsync(paymentId);
        if (payment != null)
        {
            payment.Verify(referenceNumber);
            await UpdateAsync(payment);
        }
        return payment!;
    }

    public async Task<Payment> RefundPaymentAsync(int paymentId, string reason)
    {
        var payment = await GetByIdAsync(paymentId);
        if (payment != null)
        {
            payment.Refund(reason);
            await UpdateAsync(payment);
        }
        return payment!;
    }

    public async Task<decimal> GetOrderTotalAmountAsync(int orderId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<decimal>(
            "SELECT ISNULL(TotalAmount, 0) FROM Orders WHERE Id = @OrderId AND IsDeleted = 0",
            new { OrderId = orderId }
        );
    }
}