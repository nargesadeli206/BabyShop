using System;
using System.ComponentModel.DataAnnotations;

namespace BabyShop.Application.Dtos;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int InitialStock { get; set; }

    [Required]
    [Range(1, 3, ErrorMessage = "Gender must be 1 (پسرانه), 2 (دخترانه), or 3 (مشترک)")]
    public int Gender { get; set; }

    [Required]
    [RegularExpression("^(0-3|3-6|6-12|12-24|24+)$",
        ErrorMessage = "Age range must be one of: 0-3, 3-6, 6-12, 12-24, 24+")]
    public string AgeRange { get; set; } = string.Empty;
}

public class UpdateProductDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [Range(1, 3, ErrorMessage = "Gender must be 1 (پسرانه), 2 (دخترانه), or 3 (مشترک)")]
    public int Gender { get; set; }

    [Required]
    [RegularExpression("^(0-3|3-6|6-12|12-24|24+)$",
        ErrorMessage = "Age range must be one of: 0-3, 3-6, 6-12, 12-24, 24+")]
    public string AgeRange { get; set; } = string.Empty;
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
    public DateTime CreatedAt { get; set; }

    // فیلدهای جدید
    public int GenderValue { get; set; }
    public string GenderName { get; set; } = string.Empty;
    public string AgeRangeCode { get; set; } = string.Empty;
    public string AgeRangeDisplay { get; set; } = string.Empty;

    // موجودی
    public int CurrentStock { get; set; }
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

    // فیلدهای جدید
    public string GenderName { get; set; } = string.Empty;
    public string AgeRangeDisplay { get; set; } = string.Empty;

    // موجودی
    public int CurrentStock { get; set; }
}