using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface IReportRepository
{
    Task<MonthlySalesReportDto> GetMonthlySalesReportAsync(DateTime startDate, DateTime endDate);
    Task<List<MonthlySalesReportDto>> GetYearlySalesReportAsync(int year);
    Task<List<TopProductReportDto>> GetTopProductsReportAsync(DateTime startDate, DateTime endDate, int count);
    Task<List<OrderStatusReportDto>> GetOrderStatusReportAsync();
    Task<List<NewUsersReportDto>> GetNewUsersReportAsync(DateTime startDate, DateTime endDate);
    Task<List<InventoryReportDto>> GetLowStockReportAsync(int threshold);
}