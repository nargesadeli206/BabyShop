using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BabyShop.Infrastructure.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly string _connectionString;

    public SupportTicketRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing");
    }

    public async Task<SupportTicket?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<SupportTicket>(
            "SELECT TOP 1 * FROM SupportTickets WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByUserIdAsync(int userId)
    {
        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<SupportTicket>(
            @"SELECT * FROM SupportTickets
              WHERE UserId = @UserId
              ORDER BY CreatedAt DESC",
            new { UserId = userId });
        return result.AsList();
    }

    public async Task<IReadOnlyList<SupportTicket>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<SupportTicket>(
            @"SELECT * FROM SupportTickets
              ORDER BY CreatedAt DESC");
        return result.AsList();
    }

    public async Task<SupportTicket> AddAsync(SupportTicket ticket)
    {
        using var connection = new SqlConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO SupportTickets
                (TicketNumber, Subject, Message, UserId, OrderId, ProductId, Status, Priority, Category, CreatedAt)
              VALUES
                (@TicketNumber, @Subject, @Message, @UserId, @OrderId, @ProductId, @Status, @Priority, @Category, GETUTCDATE());
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                ticket.TicketNumber,
                ticket.Subject,
                ticket.Message,
                ticket.UserId,
                ticket.OrderId,
                ticket.ProductId,
                ticket.Status,
                ticket.Priority,
                ticket.Category
            });
        ticket.Id = id;
        return ticket;
    }

    public async Task UpdateAsync(SupportTicket ticket)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE SupportTickets SET
                Status = @Status,
                Priority = @Priority,
                UpdatedAt = GETUTCDATE()
              WHERE Id = @Id",
            new { ticket.Id, ticket.Status, ticket.Priority });
    }

    public async Task<TicketReply> AddReplyAsync(TicketReply reply)
    {
        using var connection = new SqlConnection(_connectionString);

        // بدون IsDeleted — چون جدولت این ستون را ندارد
        var id = await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO TicketReplies
                (TicketId, UserId, Message, IsAdminReply, CreatedAt)
              VALUES
                (@TicketId, @UserId, @Message, @IsAdminReply, GETUTCDATE());
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            new
            {
                reply.TicketId,
                reply.UserId,
                reply.Message,
                reply.IsAdminReply
            });

        reply.Id = id;
        return reply;
    }

    public async Task<IReadOnlyList<TicketReply>> GetRepliesAsync(int ticketId)
    {
        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<TicketReply>(
            @"SELECT * FROM TicketReplies
              WHERE TicketId = @TicketId
              ORDER BY CreatedAt ASC",
            new { TicketId = ticketId });
        return result.AsList();
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM SupportTickets WHERE Status = @Status",
            new { Status = status });
    }

    public async Task<int> CountAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM SupportTickets");
    }

    public async Task<string> GenerateTicketNumberAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM SupportTickets");
        return $"TKT-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
    }
}