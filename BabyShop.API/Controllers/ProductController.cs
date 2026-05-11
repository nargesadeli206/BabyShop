using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

//[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = product.Id },
                new ApiResponse<ProductDto> { Success = true, Data = product, Message = "Product created" });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update([FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(dto);
            return Ok(new ApiResponse<ProductDto> { Success = true, Data = product, Message = "Product updated" });
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
            _logger.LogError(ex, "Error updating product");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Product {id} not found" });

        return Ok(new ApiResponse<ProductDto> { Success = true, Data = product });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ProductListDto>>>> GetPaged(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] int? gender = null, [FromQuery] string? ageRange = null)
    {
        try
        {
            var result = await _productService.GetProductsPagedAsync(pageNumber, pageSize, gender, ageRange);
            return Ok(new ApiResponse<PagedResultDto<ProductListDto>> { Success = true, Data = result });
        }
        catch (NotFoundException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("by-gender/{gender}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByGender(int gender)
    {
        var products = await _productService.GetProductsByGenderAsync(gender);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("by-age/{ageRange}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByAgeRange(string ageRange)
    {
        var products = await _productService.GetProductsByAgeRangeAsync(ageRange);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByCategory(int categoryId)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> Search([FromQuery] string term)
    {
        var products = await _productService.SearchProductsAsync(term);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new ApiResponse<object> { Success = true, Message = "Product deleted" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}