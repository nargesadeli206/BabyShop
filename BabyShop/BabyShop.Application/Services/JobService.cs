using Hangfire;
using Microsoft.Extensions.Logging;
using BabyShop.Core.Interfaces;
using BabyShop.Application.Interfaces;
using System.Text;
using System.Linq;

namespace BabyShop.Application.Services;

public class JobService : IJobService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryRepository inventoryRepository,
        ILogger<JobService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

   
    public async Task SendMonthlyReportAsync()
    {
        _logger.LogInformation(" شروع گزارش ماهانه در {Time}", DateTime.Now);

        try
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var orders = await _orderRepository.GetOrdersByDateRangeAsync(firstDayOfMonth, lastDayOfMonth);
            var totalSales = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;

            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    TotalSold = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            var lowStockProducts = await _productRepository.GetLowStockProductsAsync(5);

            var report = new StringBuilder();
            report.AppendLine(" گزارش ماهانه فروشگاه BabyShop");
            report.AppendLine($" ماه: {now.ToString("yyyy/MM")}");
            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            report.AppendLine($" کل فروش: {totalSales:N0} تومان");
            report.AppendLine($" تعداد سفارشات: {totalOrders}");
            report.AppendLine($" میانگین هر سفارش: {(totalOrders > 0 ? totalSales / totalOrders : 0):N0} تومان");
            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            report.AppendLine(" محصولات پرفروش ماه:");

            int rank = 1;
            foreach (var item in topProducts)
            {
                report.AppendLine($"  {rank++}. {item.ProductName} - {item.TotalSold} عدد");
            }

            report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            report.AppendLine($" محصولات با موجودی کم: {lowStockProducts.Count} عدد");

            _logger.LogInformation(" گزارش ماهانه تهیه شد:\n{Report}", report.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " خطا در تهیه گزارش ماهانه");
            throw;
        }
    }

   
    public async Task CheckLowStockAsync()
    {
        _logger.LogInformation(" بررسی محصولات با موجودی کم - ساعت {Time}", DateTime.Now.ToString("HH:mm"));

        try
        {
            var lowStockProducts = await _productRepository.GetLowStockProductsAsync(5);

            if (lowStockProducts.Any())
            {
                _logger.LogWarning(" {Count} محصول موجودی کم دارند:", lowStockProducts.Count);

                foreach (var product in lowStockProducts.Take(10))
                {
                    _logger.LogWarning("   - {ProductName}: موجودی {Stock}",
                        product.Name,
                        product.Inventory?.CurrentStock ?? 0);
                }
            }
            else
            {
                _logger.LogInformation(" همه محصولات موجودی کافی دارند");
            }

            var totalProducts = await _productRepository.GetAllAsync();
            var outOfStock = totalProducts.Count(p => p.Inventory?.CurrentStock <= 0);

            _logger.LogInformation(" آمار موجودی: {Total} محصول | {OutOfStock} ناموجود | {LowStock} در آستانه اتمام",
                totalProducts.Count(), outOfStock, lowStockProducts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " خطا در بررسی موجودی");
            throw;
        }
    }

    public async Task CleanupOldOrdersAsync()
    {
        _logger.LogInformation(" شروع پاکسازی هفتگی - {Date}", DateTime.Now.ToString("yyyy/MM/dd"));

        try
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var oldOrders = await _orderRepository.GetOldDeliveredOrdersAsync(sixMonthsAgo);

            _logger.LogInformation(" {Count} سفارش قدیمی برای پاکسازی یافت شد", oldOrders.Count);

            foreach (var order in oldOrders.Take(50))
            {
                order.IsArchived = true;
                order.ArchivedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                _logger.LogDebug("سفارش {OrderNumber} بایگانی شد", order.OrderNumber);
            }

            _logger.LogInformation(" پاکسازی کامل شد: {Count} سفارش بایگانی شد", oldOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " خطا در پاکسازی هفتگی");
            throw;
        }
    }
}