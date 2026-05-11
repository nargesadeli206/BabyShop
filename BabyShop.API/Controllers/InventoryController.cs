using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Create([FromBody] CreateInventoryDto dto)
    {
        try
        {
            var inventory = await _inventoryService.CreateInventoryAsync(dto);
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory, Message = "موجودی با موفقیت ایجاد شد" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpPut("increase")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Increase(int productId, int quantity)
    {
        try
        {
            var inventory = await _inventoryService.IncreaseStockAsync(productId, quantity);
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory, Message = "موجودی با موفقیت افزایش یافت" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error increasing stock for product {ProductId}", productId);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpPut("decrease")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Decrease(int productId, int quantity)
    {
        try
        {
            var inventory = await _inventoryService.DecreaseStockAsync(productId, quantity);
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory, Message = "موجودی با موفقیت کاهش یافت" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decreasing stock for product {ProductId}", productId);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetByProduct(int productId)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventory == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"موجودی برای محصول {productId} یافت نشد" });

            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory for product {ProductId}", productId);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetAll()
    {
        try
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(new ApiResponse<List<InventoryDto>> { Success = true, Data = inventories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventories");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpGet("lowstock")]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetLowStock()
    {
        try
        {
            var inventories = await _inventoryService.GetLowStockInventoriesAsync();
            return Ok(new ApiResponse<List<InventoryDto>> { Success = true, Data = inventories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock inventories");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetById(int id)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"موجودی با شناسه {id} یافت نشد" });

            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory by id {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _inventoryService.DeleteInventoryAsync(id);
            return Ok(new ApiResponse<object> { Success = true, Message = "موجودی با موفقیت حذف شد" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای داخلی سرور" });
        }
    }
}