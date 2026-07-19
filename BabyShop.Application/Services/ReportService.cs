using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BabyShop.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISupportTicketRepository? _ticketRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        ILogger<ReportService> logger,
        ISupportTicketRepository? ticketRepository = null)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<DashboardReportDto> GetDashboardAsync()
    {
        var orders = (await _orderRepository.GetAllAsync()).ToList();
        var products = (await _productRepository.GetAllAsync()).ToList();
        var users = (await _userRepository.GetAllAsync()).ToList();

        var today = DateTime.UtcNow.Date;
        var ordersToday = orders.Where(o => o.CreatedAt.Date == today).ToList();

        var lowStock = 0;
        try { lowStock = (await _reportRepository.GetLowStockReportAsync(5)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "low stock dashboard"); }

        var openTickets = 0;
        if (_ticketRepository != null)
        {
            try { openTickets = await _ticketRepository.CountByStatusAsync("Open"); }
            catch { }
        }

        return new DashboardReportDto
        {
            TotalOrders = orders.Count,
            TotalProducts = products.Count,
            TotalUsers = users.Count,
            TotalSales = orders.Sum(o => o.TotalAmount),
            LowStockCount = lowStock,
            OpenTickets = openTickets,
            OrdersToday = ordersToday.Count,
            SalesToday = ordersToday.Sum(o => o.TotalAmount)
        };
    }

    public Task<List<MonthlySalesReportDto>> GetMonthlySalesAsync(int? year = null)
        => _reportRepository.GetYearlySalesReportAsync(year ?? DateTime.UtcNow.Year);

    public async Task<List<MonthlySalesReportDto>> GetYearlySalesAsync()
    {
        var thisYear = DateTime.UtcNow.Year;
        var list = new List<MonthlySalesReportDto>();
        foreach (var y in Enumerable.Range(thisYear - 4, 5))
        {
            var months = await _reportRepository.GetYearlySalesReportAsync(y);
            list.Add(new MonthlySalesReportDto
            {
                Label = y.ToString(),
                OrderCount = months.Sum(m => m.OrderCount),
                TotalAmount = months.Sum(m => m.TotalAmount)
            });
        }
        return list;
    }

    public Task<List<TopProductReportDto>> GetTopProductsAsync(int take = 10)
    {
        var end = DateTime.UtcNow.AddDays(1);
        var start = end.AddYears(-1);
        return _reportRepository.GetTopProductsReportAsync(start, end, take);
    }

    public Task<List<OrderStatusReportDto>> GetOrderStatusAsync()
        => _reportRepository.GetOrderStatusReportAsync();

    public Task<List<NewUsersReportDto>> GetNewUsersAsync(int days = 30)
    {
        var end = DateTime.UtcNow.AddDays(1);
        var start = DateTime.UtcNow.Date.AddDays(-days);
        return _reportRepository.GetNewUsersReportAsync(start, end);
    }

    public Task<List<InventoryReportDto>> GetLowStockAsync()
        => _reportRepository.GetLowStockReportAsync(5);
}