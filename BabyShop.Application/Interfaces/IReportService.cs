using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface IReportService
{
    Task<DashboardReportDto> GetDashboardAsync();
    Task<List<MonthlySalesReportDto>> GetMonthlySalesAsync(int? year = null);
    Task<List<MonthlySalesReportDto>> GetYearlySalesAsync();
    Task<List<TopProductReportDto>> GetTopProductsAsync(int take = 10);
    Task<List<OrderStatusReportDto>> GetOrderStatusAsync();
    Task<List<NewUsersReportDto>> GetNewUsersAsync(int days = 30);
    Task<List<InventoryReportDto>> GetLowStockAsync();
}