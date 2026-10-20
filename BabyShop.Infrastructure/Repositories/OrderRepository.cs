using BabyShop.Core.Entities;
using BabyShop.Core.Entities.Base;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyShop.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        // تزریق IConfiguration برای خواندن ConnectionString از appsettings.json
        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection string is missing.");
        }

        public async Task<Order> AddAsync(Order entity)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // استفاده از Transaction برای اطمینان از ثبت کامل سفارش و اقلام (بیزنسی)
            using var transaction = connection.BeginTransaction();
            try
            {
                var sqlOrder = @"
                    INSERT INTO Orders (UserId, Status, SubTotal, Tax, ShippingCost, Discount, TotalAmount, 
                                        ShippingAddress, PhoneNumber, Email, CustomerNote, AdminNote, 
                                        OrderNumber, CancellationReason, PaidAt, ShippedAt, DeliveredAt, 
                                        CreatedAt, IsDeleted, IsReminderSent, IsArchived) 
                    VALUES (@UserId, @Status, @SubTotal, @Tax, @ShippingCost, @Discount, @TotalAmount, 
                            @ShippingAddress, @PhoneNumber, @Email, @CustomerNote, @AdminNote, 
                            @OrderNumber, @CancellationReason, @PaidAt, @ShippedAt, @DeliveredAt, 
                            GETDATE(), 0, 0, 0);
                    SELECT SCOPE_IDENTITY();";

                // ثبت سفارش اصلی و گرفتن آیدی آن
                var orderId = await connection.ExecuteScalarAsync<int>(sqlOrder, entity, transaction);

                // ست کردن آیدی در موجودیت
                typeof(BaseEntity).GetProperty("Id")?.SetValue(entity, orderId);

                // اگر سفارش اقلامی داشت، آن‌ها را هم ثبت می‌کنیم
                if (entity.Items != null && entity.Items.Any())
                {
                    var sqlItems = @"
                        INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice, CreatedAt, IsDeleted) 
                        VALUES (@OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice, GETDATE(), 0)";

                    foreach (var item in entity.Items)
                    {
                        await connection.ExecuteAsync(sqlItems, new
                        {
                            OrderId = orderId,
                            item.ProductId,
                            item.ProductName,
                            item.Quantity,
                            item.UnitPrice
                        }, transaction);
                    }
                }

                // تایید نهایی تراکنش
                transaction.Commit();
                return entity;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM Orders WHERE Id = @Id AND IsDeleted = 0", new { Id = id });
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            // در پنل ادمین نیاز داریم نام کاربر رو هم ببینیم (LEFT JOIN)
            var sql = @"
                SELECT o.*, u.FullName as UserName 
                FROM Orders o 
                LEFT JOIN Users u ON o.UserId = u.Id 
                WHERE o.IsDeleted = 0 
                ORDER BY o.CreatedAt DESC";

            var orders = await connection.QueryAsync<Order>(sql);
            return orders.ToList();
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var order = await GetByIdAsync(id);
            if (order == null) return null;

            var items = await connection.QueryAsync<OrderItem>(
                "SELECT * FROM OrderItems WHERE OrderId = @OrderId AND IsDeleted = 0", new { OrderId = id });

            // اضافه کردن آیتم‌ها به آبجکت اصلی
            foreach (var item in items)
            {
                order.AddItem(item);
            }

            return order;
        }

        public async Task UpdateAsync(Order entity)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Orders 
                SET Status = @Status, 
                    CancellationReason = @CancellationReason,
                    PaidAt = @PaidAt,
                    ShippedAt = @ShippedAt,
                    DeliveredAt = @DeliveredAt,
                    AdminNote = @AdminNote,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(Order entity)
        {
            using var connection = new SqlConnection(_connectionString);
            // Soft Delete (حذف نرم)
            await connection.ExecuteAsync(
                "UPDATE Orders SET IsDeleted = 1, DeletedAt = GETDATE() WHERE Id = @Id", new { Id = entity.Id });
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByUserAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE UserId = @UserId AND IsDeleted = 0 ORDER BY CreatedAt DESC",
                new { UserId = userId });
            return orders.ToList();
        }

        // --- پیاده سازی سایر متدهای اینترفیس (برای اینکه ارور CS0535 ندهی) ---

        public async Task<IReadOnlyList<Order>> FindAsync(Func<Order, bool> predicate)
        {
            var all = await GetAllAsync();
            return all.Where(predicate).ToList();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return (await GetByIdAsync(id)) != null;
        }

        public async Task<int> CountAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders WHERE IsDeleted = 0");
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(string status)
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE Status = @Status AND IsDeleted = 0", new { Status = status });
            return orders.ToList();
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate AND IsDeleted = 0",
                new { StartDate = startDate, EndDate = endDate });
            return orders.ToList();
        }

        public async Task<List<Order>> GetOldDeliveredOrdersAsync(DateTime date)
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE Status = 'Delivered' AND DeliveredAt <= @Date AND IsDeleted = 0",
                new { Date = date });
            return orders.ToList();
        }

        public async Task<List<Order>> GetAbandonedCartsAsync(DateTime date)
        {
            using var connection = new SqlConnection(_connectionString);
            var orders = await connection.QueryAsync<Order>(
                "SELECT * FROM Orders WHERE Status = 'Pending' AND CreatedAt <= @Date AND IsDeleted = 0",
                new { Date = date });
            return orders.ToList();
        }
    }
}