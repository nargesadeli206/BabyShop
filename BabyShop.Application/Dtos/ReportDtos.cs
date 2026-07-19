namespace BabyShop.Application.Dtos;

public class DashboardReportDto
{
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalSales { get; set; }
    public int LowStockCount { get; set; }
    public int OpenTickets { get; set; }
    public int OrdersToday { get; set; }
    public decimal SalesToday { get; set; }
}

public class MonthlySalesReportDto
{
    public string Label { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TopProductReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderStatusReportDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class NewUsersReportDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class InventoryReportDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStockLevel { get; set; }
}