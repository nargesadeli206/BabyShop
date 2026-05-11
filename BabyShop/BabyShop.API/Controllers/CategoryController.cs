using Microsoft.AspNetCore.Mvc;
using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Core.Exceptions;

namespace BabyShop.API.Controllers;

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
                new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Data = category,
                    Message = "Category created successfully"
                });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating category"
            });
        }
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update([FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(dto);
            return Ok(new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = category,
                Message = "Category updated successfully"
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating category"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Get(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Category with id {id} not found"
                });

            return Ok(new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = category
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving category"
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving categories"
            });
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<CategoryDto>>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _categoryService.GetCategoriesPagedAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResultDto<CategoryDto>>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged categories");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving categories"
            });
        }
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetBySlug(string slug)
    {
        try
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            if (category == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Category with slug '{slug}' not found"
                });

            return Ok(new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = category
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving category"
            });
        }
    }

    [HttpGet("main")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetMainCategories()
    {
        try
        {
            var categories = await _categoryService.GetMainCategoriesAsync();
            return Ok(new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting main categories");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving main categories"
            });
        }
    }

    [HttpGet("{parentId}/subcategories")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetSubCategories(int parentId)
    {
        try
        {
            var categories = await _categoryService.GetSubCategoriesAsync(parentId);
            return Ok(new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for parent {ParentId}", parentId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving subcategories"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Category deleted successfully"
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting category"
            });
        }
    }

    [HttpGet("{id}/hasproducts")]
    public async Task<ActionResult<ApiResponse<bool>>> HasProducts(int id)
    {
        try
        {
            var hasProducts = await _categoryService.HasProductsAsync(id);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = hasProducts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category {Id} has products", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while checking category"
            });
        }
    }

    [HttpGet("{id}/hassubcategories")]
    public async Task<ActionResult<ApiResponse<bool>>> HasSubCategories(int id)
    {
        try
        {
            var hasSubs = await _categoryService.HasSubCategoriesAsync(id);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = hasSubs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category {Id} has subcategories", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while checking category"
            });
        }
    }
}