using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace BabyShop.Application.Dtos;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;

    // تو فرانت‌اند Gender به صورت استرینگ میاد، پس بهتره اینجا هم استرینگ باشه
    public string Gender { get; set; } = "1";
    public string AgeRange { get; set; } = string.Empty;

    public string? InventoryDetails { get; set; }
    public int InitialStock { get; set; }
    public List<IFormFile>? Images { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int Gender { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<IFormFile>? NewImages { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public int GenderValue { get; set; }
    public string GenderName { get; set; } = string.Empty;

    // --- فیلد کلیدی برای فیلترهای فرانت‌اند ---
    public string Gender { get; set; } = string.Empty;

    public string AgeRangeCode { get; set; } = string.Empty;
    public string AgeRangeDisplay { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SoldCount { get; set; }
    public string GenderName { get; set; } = string.Empty;
    public string AgeRangeDisplay { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public string? ImageUrl { get; set; }

    // --- فیلدهای فیلترینگ ---
    public int CategoryId { get; set; }
    public string? Gender { get; set; }
}