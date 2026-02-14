using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto);
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<List<CategoryDto>> GetMainCategoriesAsync();
    Task DeleteCategoryAsync(int id);
}