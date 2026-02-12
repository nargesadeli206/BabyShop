using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.Entities.Products;
using BabyShop.Services.BabyShop.Application.Dtos.Products;

namespace BabyShop.BabyShop.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateProductDto dto)
        {
            var product = new ProductEntity(dto.Name, dto.Price);
            await _repository.AddAsync(product);
            return product.Id;
        }

        public async Task UpdateAsync(UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(dto.Id);
            if (product == null) throw new Exception("Product not found");

            product.Update(dto.Name, dto.Price);
            await _repository.UpdateAsync(product);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
        }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();
            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToList();
        }
    }
}