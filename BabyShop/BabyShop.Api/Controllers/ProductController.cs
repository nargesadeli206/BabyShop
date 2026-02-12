using BabyShop.Application.Dtos.Products;
using BabyShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return Ok(product);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(request);
        return Ok(product);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}