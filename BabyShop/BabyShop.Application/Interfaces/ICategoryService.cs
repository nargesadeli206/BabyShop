using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface ICategoryService
{
    // Create
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);

    // Update
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto);

    // Get single
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug);

    // Get lists
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<List<CategoryDto>> GetMainCategoriesAsync();
    Task<List<CategoryDto>> GetSubCategoriesAsync(int parentId);

    // Paged
    Task<PagedResultDto<CategoryDto>> GetCategoriesPagedAsync(int pageNumber, int pageSize);

    // Delete
    Task DeleteCategoryAsync(int id);

    // Check
    Task<bool> HasProductsAsync(int id);
    Task<bool> HasSubCategoriesAsync(int id);
}