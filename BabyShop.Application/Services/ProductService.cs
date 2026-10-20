using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using BabyShop.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BabyShop.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IInventoryService inventoryService, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var gender = Gender.FromValue(int.TryParse(dto.Gender, out var g) ? g : 3);

            // اگر AgeRange خالی بود از یه دیفالت استفاده می‌کنیم که خطا نده
            var firstAgeRangeCode = string.IsNullOrEmpty(dto.AgeRange) ? "0-3" : dto.AgeRange.Split(',')[0];
            var ageRange = AgeRange.FromCode(firstAgeRangeCode);

            // ۱. ساخت اولیه محصول
            var product = new Product(dto.Name, dto.Description, dto.Price, dto.CategoryId, gender, ageRange);

            // ۲. تنظیم Slug (حل ارور کلمات عجیب مثل ????? و تکراری بودن)
            var safeSlug = dto.Slug;
            if (string.IsNullOrWhiteSpace(safeSlug) || safeSlug.Contains("?"))
            {
                safeSlug = $"product-{DateTime.Now.Ticks}";
            }
            else
            {
                var randomCode = new Random().Next(1000, 9999);
                safeSlug = $"{safeSlug}-{randomCode}";
            }

            product.SetSlug(safeSlug);

            // ۳. ذخیره فیزیکی تصاویر در هاست (wwwroot)
            if (dto.Images != null && dto.Images.Any())
            {
                var savedPaths = new List<string>();
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in dto.Images)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    savedPaths.Add($"/uploads/products/{fileName}");
                }
                product.SetImageUrl(string.Join("|", savedPaths));
                product.SetMainImage(savedPaths[0]);
            }

            // ذخیره محصول در دیتابیس
            var createdProduct = await _productRepository.AddAsync(product);

            // ۴. ثبت موجودی هوشمند در انبار (بر اساس رده‌های سنی)
            if (!string.IsNullOrEmpty(dto.InventoryDetails))
            {
                try
                {
                    var inventoryList = JsonSerializer.Deserialize<List<AgeRangeStockDto>>(dto.InventoryDetails);

                    if (inventoryList != null && inventoryList.Any())
                    {
                        foreach (var item in inventoryList)
                        {
                            if (item.Stock > 0)
                            {
                                await _inventoryService.IncreaseStockAsync(createdProduct.Id, item.AgeRange, item.Stock);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در پردازش لیست موجودی سایزها.");
                    await _inventoryService.IncreaseStockAsync(createdProduct.Id, firstAgeRangeCode, dto.InitialStock);
                }
            }
            else
            {
                await _inventoryService.IncreaseStockAsync(createdProduct.Id, dto.InitialStock);
            }

            return MapToDto(createdProduct);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _productRepository.GetByIdWithCategoryAsync(id);
            return p == null ? null : MapToDto(p);
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithCategoryAsync();
            return products.Select(MapToDto).ToList();
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CurrentStock = product.StockQuantity,

                // این دو فیلد برای کارکرد فیلترهای فرانت‌اِند حیاتی هستند
                CategoryId = product.CategoryId,
                Gender = product.Gender?.Value.ToString() ?? "3"
            };
        }

        public async Task DeleteProductAsync(int id) => await _productRepository.DeleteProductAsync(id);
        public async Task<List<ProductDto>> SearchProductsAsync(string term) => (await _productRepository.SearchAsync(term)).Select(MapToDto).ToList();

        // متدهای پیاده‌سازی نشده
        public Task<ProductDto> UpdateProductAsync(UpdateProductDto dto) => throw new NotImplementedException();
        public Task<ProductDto> UploadProductImageAsync(int id, string url) => throw new NotImplementedException();
        public Task<PagedResultDto<ProductListDto>> GetProductsPagedAsync(int pg, int sz, int? g, string? a) => throw new NotImplementedException();
        public Task<List<ProductDto>> GetProductsByCategoryAsync(int c) => throw new NotImplementedException();
        public Task<List<ProductDto>> GetProductsByGenderAsync(int g) => throw new NotImplementedException();
        public Task<List<ProductDto>> GetProductsByAgeRangeAsync(string a) => throw new NotImplementedException();
    }

    public class AgeRangeStockDto
    {
        public string AgeRange { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}