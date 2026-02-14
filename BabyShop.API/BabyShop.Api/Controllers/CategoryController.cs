using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = category.Id },
                new ApiResponse<CategoryDto> { Success = true, Data = category });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update([FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(dto);
            return Ok(new ApiResponse<CategoryDto> { Success = true, Data = category });
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Get(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Category {id} not found" });

        return Ok(new ApiResponse<CategoryDto> { Success = true, Data = category });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetBySlug(string slug)
    {
        var category = await _categoryService.GetCategoryBySlugAsync(slug);
        if (category == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Category '{slug}' not found" });

        return Ok(new ApiResponse<CategoryDto> { Success = true, Data = category });
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(new ApiResponse<List<CategoryDto>> { Success = true, Data = categories });
    }

    [HttpGet("main")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetMain()
    {
        var categories = await _categoryService.GetMainCategoriesAsync();
        return Ok(new ApiResponse<List<CategoryDto>> { Success = true, Data = categories });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new ApiResponse<object> { Success = true, Message = "Category deleted" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}