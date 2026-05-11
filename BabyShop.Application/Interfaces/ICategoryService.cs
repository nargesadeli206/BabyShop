using BabyShop.Application.Dtos;
using BabyShop.Core.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetCategoryDtoByIdAsync(int id);
    Task<CategoryDto?> GetCategoryDtoBySlugAsync(string slug);
    Task<List<CategoryDto>> GetAllCategoryDtosAsync();
    Task<List<CategoryDto>> GetMainCategoryDtosAsync();
    Task<List<CategoryDto>> GetSubCategoryDtosAsync(int parentId);
    Task<Dtos.PagedResultDto<CategoryDto>> GetCategoriesPagedAsync(int pageNumber, int pageSize);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto dto);
    Task SoftDeleteCategoryAsync(int id);
    Task<bool> HasProductsAsync(int id);
    Task<bool> HasSubCategoriesAsync(int id);
}