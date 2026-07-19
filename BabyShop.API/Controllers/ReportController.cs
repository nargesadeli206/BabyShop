using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

/// <summary>
/// فقط Admin — گزارش‌ها از Application (IReportService) می‌آیند.
/// DTOها در Application.Dtos هستند (Clean Architecture).
/// </summary>
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var data = await _reportService.GetDashboardAsync();
            return Ok(new ApiResponse<DashboardReportDto> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report dashboard failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("sales/monthly")]
    public async Task<IActionResult> MonthlySales([FromQuery] int? year = null)
    {
        try
        {
            var data = await _reportService.GetMonthlySalesAsync(year);
            return Ok(new ApiResponse<List<MonthlySalesReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report monthly sales failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("sales/yearly")]
    public async Task<IActionResult> YearlySales()
    {
        try
        {
            var data = await _reportService.GetYearlySalesAsync();
            return Ok(new ApiResponse<List<MonthlySalesReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report yearly sales failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] int take = 10)
    {
        try
        {
            var data = await _reportService.GetTopProductsAsync(take);
            return Ok(new ApiResponse<List<TopProductReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report top products failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("order-status")]
    public async Task<IActionResult> OrderStatus()
    {
        try
        {
            var data = await _reportService.GetOrderStatusAsync();
            return Ok(new ApiResponse<List<OrderStatusReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report order status failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("new-users")]
    public async Task<IActionResult> NewUsers([FromQuery] int days = 30)
    {
        try
        {
            var data = await _reportService.GetNewUsersAsync(days);
            return Ok(new ApiResponse<List<NewUsersReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report new users failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var data = await _reportService.GetLowStockAsync();
            return Ok(new ApiResponse<List<InventoryReportDto>> { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report low stock failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}