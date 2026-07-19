namespace BabyShop.Application.Interfaces.Services;

public interface IJobService
{
    Task SendMonthlyReportAsync();
    Task CheckLowStockAsync();
    Task CleanupOldOrdersAsync();
    Task CleanupAbandonedCartsAsync();
    Task GenerateDailySalesReportAsync();     
    Task GenerateYearlySalesReportAsync();    
    Task GenerateTopSellingProductsReportAsync(); 
    Task GenerateInventoryStatusReportAsync();     
    Task GenerateNewUsersReportAsync();
}