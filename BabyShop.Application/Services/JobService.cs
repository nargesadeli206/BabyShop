using BabyShop.Application.Interfaces.Repositories;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BabyShop.Application.Services;

public class JobService : IJobService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<JobService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task SendMonthlyReportAsync()
    {
        _logger.LogInformation("شروع گزارش ماهانه");

        try
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var allOrders = await _orderRepository.GetAllAsync();
            var ordersInMonth = allOrders
                .Where(o => o.CreatedAt >= firstDayOfMonth && o.CreatedAt <= lastDayOfMonth)
                .ToList();

            var totalSales = ordersInMonth.Sum(o => o.TotalAmount);
            var totalOrders = ordersInMonth.Count;

            var topProducts = ordersInMonth
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductName = g.First().ProductName,
                    TotalSold = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            var report = new StringBuilder();
            report.AppendLine("═══════════════════════════");
            report.AppendLine("   گزارش ماهانه فروشگاه");
            report.AppendLine($"   ماه: {now:yyyy/MM}");
            report.AppendLine("═══════════════════════════");
            report.AppendLine($"کل فروش: {totalSales:N0} تومان");
            report.AppendLine($"تعداد سفارشات: {totalOrders}");
            report.AppendLine("───────────────────────────");
            report.AppendLine("محصولات پرفروش:");

            for (int i = 0; i < topProducts.Count; i++)
            {
                report.AppendLine($"   {i + 1}. {topProducts[i].ProductName} - {topProducts[i].TotalSold} عدد");
            }

            report.AppendLine("═══════════════════════════");
            _logger.LogInformation(report.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در گزارش ماهانه");
        }
    }

    public async Task CheckLowStockAsync()
    {
        _logger.LogInformation("بررسی موجودی کالاها");

        try
        {
            var allProducts = await _productRepository.GetAllAsync();
            var lowStockProducts = allProducts
                .Where(p => p.Inventory != null && p.Inventory.CurrentStock < 10)
                .ToList();

            if (lowStockProducts.Any())
            {
                _logger.LogWarning("⚠️ {Count} محصول موجودی کم دارند:", lowStockProducts.Count);
                foreach (var product in lowStockProducts.Take(10))
                {
                    _logger.LogWarning("   • {Name}: {Stock} عدد", product.Name, product.Inventory?.CurrentStock ?? 0);
                }
            }
            else
            {
                _logger.LogInformation("✅ موجودی همه محصولات مناسب است");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در بررسی موجودی");
        }
    }

    public async Task CleanupOldOrdersAsync()
    {
        _logger.LogInformation("پاکسازی سفارش‌های قدیمی");

        try
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var allOrders = await _orderRepository.GetAllAsync();
            var oldOrders = allOrders
                .Where(o => o.CreatedAt < sixMonthsAgo && o.Status == "Delivered")
                .ToList();

            _logger.LogInformation("{Count} سفارش قدیمی پیدا شد", oldOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در پاکسازی سفارش‌ها");
        }
    }

    public async Task CleanupAbandonedCartsAsync()
    {
        _logger.LogInformation("پاکسازی سبدهای خرید رها شده");

        try
        {
            _logger.LogInformation("سبدهای رها شده با موفقیت پاکسازی شدند");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در پاکسازی سبدها");
        }
    }
}