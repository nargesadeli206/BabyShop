using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BabyShop.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly string _cs;

    public ReportRepository(IConfiguration configuration)
    {
        _cs = configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DefaultConnection missing");
    }

    public async Task<MonthlySalesReportDto> GetMonthlySalesReportAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = new SqlConnection(_cs);
        var row = await connection.QueryFirstOrDefaultAsync<(int Cnt, decimal Total)>(
            @"SELECT COUNT(1) AS Cnt, ISNULL(SUM(TotalAmount),0) AS Total
              FROM Orders
              WHERE ISNULL(IsDeleted,0)=0 AND CreatedAt >= @Start AND CreatedAt < @End",
            new { Start = startDate, End = endDate });

        return new MonthlySalesReportDto
        {
            Label = $"{startDate:yyyy-MM}",
            OrderCount = row.Cnt,
            TotalAmount = row.Total
        };
    }

    public async Task<List<MonthlySalesReportDto>> GetYearlySalesReportAsync(int year)
    {
        using var connection = new SqlConnection(_cs);
        var rows = await connection.QueryAsync<(int Month, int Cnt, decimal Total)>(
            @"SELECT MONTH(CreatedAt) AS Month, COUNT(1) AS Cnt, ISNULL(SUM(TotalAmount),0) AS Total
              FROM Orders
              WHERE ISNULL(IsDeleted,0)=0 AND YEAR(CreatedAt)=@Year
              GROUP BY MONTH(CreatedAt)",
            new { Year = year });

        var dict = rows.ToDictionary(r => r.Month, r => r);
        return Enumerable.Range(1, 12).Select(m =>
        {
            dict.TryGetValue(m, out var r);
            return new MonthlySalesReportDto
            {
                Label = $"{year}-{m:D2}",
                OrderCount = r.Cnt,
                TotalAmount = r.Total
            };
        }).ToList();
    }

    public async Task<List<TopProductReportDto>> GetTopProductsReportAsync(DateTime startDate, DateTime endDate, int count)
    {
        using var connection = new SqlConnection(_cs);
        var rows = await connection.QueryAsync<TopProductReportDto>(
            @"SELECT TOP (@Count)
                     oi.ProductId,
                     MAX(oi.ProductName) AS ProductName,
                     SUM(oi.Quantity) AS QuantitySold,
                     SUM(oi.UnitPrice * oi.Quantity) AS Revenue
              FROM OrderItems oi
              INNER JOIN Orders o ON o.Id = oi.OrderId
              WHERE ISNULL(o.IsDeleted,0)=0
                AND o.CreatedAt >= @Start AND o.CreatedAt < @End
              GROUP BY oi.ProductId
              ORDER BY SUM(oi.Quantity) DESC",
            new { Count = count, Start = startDate, End = endDate });
        return rows.AsList();
    }

    public async Task<List<OrderStatusReportDto>> GetOrderStatusReportAsync()
    {
        using var connection = new SqlConnection(_cs);
        var rows = await connection.QueryAsync<OrderStatusReportDto>(
            @"SELECT ISNULL(NULLIF(LTRIM(RTRIM(Status)),''),'Unknown') AS Status,
                     COUNT(1) AS [Count]
              FROM Orders WHERE ISNULL(IsDeleted,0)=0
              GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(Status)),''),'Unknown')
              ORDER BY COUNT(1) DESC");
        return rows.AsList();
    }

    public async Task<List<NewUsersReportDto>> GetNewUsersReportAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = new SqlConnection(_cs);
        var rows = await connection.QueryAsync<(DateTime Day, int Cnt)>(
            @"SELECT CAST(CreatedAt AS DATE) AS Day, COUNT(1) AS Cnt
              FROM Users
              WHERE ISNULL(IsDeleted,0)=0 AND CreatedAt >= @Start AND CreatedAt < @End
              GROUP BY CAST(CreatedAt AS DATE)
              ORDER BY CAST(CreatedAt AS DATE)",
            new { Start = startDate, End = endDate });

        return rows.Select(r => new NewUsersReportDto
        {
            Label = r.Day.ToString("yyyy-MM-dd"),
            Count = r.Cnt
        }).ToList();
    }

    public async Task<List<InventoryReportDto>> GetLowStockReportAsync(int threshold)
    {
        using var connection = new SqlConnection(_cs);
        try
        {
            var rows = await connection.QueryAsync<InventoryReportDto>(
                @"SELECT i.ProductId,
                         ISNULL(p.Name, CONCAT('Product #', i.ProductId)) AS ProductName,
                         i.CurrentStock,
                         ISNULL(i.MinimumStockLevel, ISNULL(i.ReorderPoint, @Threshold)) AS MinimumStockLevel
                  FROM Inventories i
                  LEFT JOIN Products p ON p.Id = i.ProductId
                  WHERE ISNULL(i.IsDeleted,0)=0
                    AND i.CurrentStock <= ISNULL(i.MinimumStockLevel, ISNULL(i.ReorderPoint, @Threshold))
                  ORDER BY i.CurrentStock ASC",
                new { Threshold = threshold });
            return rows.AsList();
        }
        catch
        {
            var rows = await connection.QueryAsync<InventoryReportDto>(
                @"SELECT i.ProductId,
                         ISNULL(p.Name, CONCAT('Product #', i.ProductId)) AS ProductName,
                         i.CurrentStock,
                         ISNULL(i.ReorderPoint, @Threshold) AS MinimumStockLevel
                  FROM Inventories i
                  LEFT JOIN Products p ON p.Id = i.ProductId
                  WHERE ISNULL(i.IsDeleted,0)=0
                    AND i.CurrentStock <= ISNULL(i.ReorderPoint, @Threshold)
                  ORDER BY i.CurrentStock ASC",
                new { Threshold = threshold });
            return rows.AsList();
        }
    }
}