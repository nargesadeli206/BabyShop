namespace BabyShop.Application.Interfaces.Services;

public interface IJobService
{
    Task SendMonthlyReportAsync();
    Task CheckLowStockAsync();
    Task CleanupOldOrdersAsync();
    Task CleanupAbandonedCartsAsync();
}