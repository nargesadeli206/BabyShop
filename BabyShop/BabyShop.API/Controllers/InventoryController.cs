using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
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
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("increase")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Increase([FromBody] UpdateInventoryDto dto)
    {
        try
        {
            var inventory = await _inventoryService.IncreaseStockAsync(dto);
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("decrease")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> Decrease([FromBody] UpdateInventoryDto dto)
    {
        try
        {
            var inventory = await _inventoryService.DecreaseStockAsync(dto);
            return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponse<InventoryDto>>> GetByProduct(int productId)
    {
        var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
        if (inventory == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Inventory for product {productId} not found" });

        return Ok(new ApiResponse<InventoryDto> { Success = true, Data = inventory });
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetAll()
    {
        var inventories = await _inventoryService.GetAllInventoriesAsync();
        return Ok(new ApiResponse<List<InventoryDto>> { Success = true, Data = inventories });
    }

    [HttpGet("lowstock")]
    public async Task<ActionResult<ApiResponse<List<InventoryDto>>>> GetLowStock()
    {
        var inventories = await _inventoryService.GetLowStockInventoriesAsync();
        return Ok(new ApiResponse<List<InventoryDto>> { Success = true, Data = inventories });
    }
}