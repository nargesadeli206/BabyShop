using BabyShop.Dtos;
using BabyShop.Entities;
using BabyShop.Interfaces;
namespace BabyShop.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price
            };

            await _repository.AddAsync(product);
        }
    }
}