using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[ApiController]
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
            var response = new ApiResponse<ProductDto>
            {
                Success = true,
                Data = product,
                Message = "Product created successfully"
            };
            return CreatedAtAction(nameof(Get), new { id = product.Id }, response);
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
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
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
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Product {id} not found" });

        return Ok(new ApiResponse<ProductDto> { Success = true, Data = product });
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll(
        [FromQuery] int? gender,
        [FromQuery] string? ageRange)
    {
        try
        {
            List<ProductDto> products;

            if (gender.HasValue)
            {
                products = await _productService.GetProductsByGenderAsync(gender.Value);
            }
            else if (!string.IsNullOrEmpty(ageRange))
            {
                products = await _productService.GetProductsByAgeRangeAsync(ageRange);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
        }
        catch (DomainException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ProductListDto>>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? gender = null,
        [FromQuery] string? ageRange = null)
    {
        try
        {
            var result = await _productService.GetProductsPagedAsync(pageNumber, pageSize, gender, ageRange);
            return Ok(new ApiResponse<PagedResultDto<ProductListDto>> { Success = true, Data = result });
        }
        catch (DomainException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("by-gender/{gender}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByGender(int gender)
    {
        try
        {
            var products = await _productService.GetProductsByGenderAsync(gender);
            return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
        }
        catch (DomainException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("by-age/{ageRange}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByAgeRange(string ageRange)
    {
        try
        {
            var products = await _productService.GetProductsByAgeRangeAsync(ageRange);
            return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
        }
        catch (DomainException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetByCategory(int categoryId)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId);
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    [HttpGet("search")]
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