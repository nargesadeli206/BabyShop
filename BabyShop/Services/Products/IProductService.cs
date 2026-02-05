using BabyShop.Dtos;

namespace BabyShop.Services.Products
{
    public interface IProductService
    {
        Task CreateAsync(CreateProductDto dto);
    }
}