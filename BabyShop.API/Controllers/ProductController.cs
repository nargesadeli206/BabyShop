using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
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

    // ۱. ایجاد محصول جدید (پشتیبانی از عکس و متن با FromForm)
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromForm] CreateProductDto dto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(dto);

            return CreatedAtAction(nameof(Get), new { id = product.Id },
                new ApiResponse<ProductDto> { Success = true, Data = product, Message = "محصول با موفقیت ایجاد شد" });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور در ایجاد محصول" });
        }
    }

    // ۲. ویرایش محصول
    [HttpPut]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update([FromForm] UpdateProductDto dto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(dto);
            return Ok(new ApiResponse<ProductDto> { Success = true, Data = product, Message = "محصول بروزرسانی شد" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    // ۳. دریافت محصول تکی
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Get(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"محصول یافت نشد" });

        return Ok(new ApiResponse<ProductDto> { Success = true, Data = product });
    }

    // ۴. دریافت لیست کل محصولات
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(new ApiResponse<List<ProductDto>> { Success = true, Data = products });
    }

    // ۵. دریافت محصولات به صورت صفحه‌بندی شده
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPaged");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    // ۶. حذف محصول
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new ApiResponse<object> { Success = true, Message = "محصول حذف شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Delete product");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }

    // ۷. آپلود عکس تکی
    [HttpPost("{id}/upload-image")]
    [Authorize(Policy = "ManagerOnly")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UploadImage(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "فایلی انتخاب نشده است" });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var product = await _productService.UploadProductImageAsync(id, $"/uploads/products/{fileName}");
            return Ok(new ApiResponse<ProductDto> { Success = true, Data = product, Message = "عکس با موفقیت آپلود شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "خطای سرور" });
        }
    }
}