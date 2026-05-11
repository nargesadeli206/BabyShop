using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq.Expressions;

namespace BabyShop.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // ============ متدهای پایه ============

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Category>(
            "SELECT * FROM Categories WHERE IsDeleted = 0 ORDER BY DisplayOrder"
        );

        return result.AsList();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Name", entity.Name);
        parameters.Add("@Slug", entity.Slug);
        parameters.Add("@Description", entity.Description);
        parameters.Add("@ParentCategoryId", entity.ParentCategoryId);
        parameters.Add("@DisplayOrder", entity.DisplayOrder);
        parameters.Add("@IsActive", entity.IsActive ? 1 : 0);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "INSERT INTO Categories (Name, Slug, Description, ParentCategoryId, DisplayOrder, IsActive, IsDeleted, CreatedAt) " +
            "VALUES (@Name, @Slug, @Description, @ParentCategoryId, @DisplayOrder, @IsActive, 0, GETDATE()); " +
            "SET @Id = SCOPE_IDENTITY();",
            parameters);

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Category entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Categories SET " +
            "Name = @Name, " +
            "Slug = @Slug, " +
            "Description = @Description, " +
            "ParentCategoryId = @ParentCategoryId, " +
            "DisplayOrder = @DisplayOrder, " +
            "IsActive = @IsActive, " +
            "UpdatedAt = GETDATE() " +
            "WHERE Id = @Id AND IsDeleted = 0",
            new
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                ParentCategoryId = entity.ParentCategoryId,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive ? 1 : 0
            }
        );
    }

    public async Task DeleteAsync(Category entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UPDATE Categories SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = entity.Id }
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var category = await GetByIdAsync(id);
        return category != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Categories WHERE IsDeleted = 0"
        );
    }

    public async Task<IReadOnlyList<Category>> GetPagedAsync(int page, int pageSize)
    {
        using var connection = new SqlConnection(_connectionString);

        var skip = (page - 1) * pageSize;

        var result = await connection.QueryAsync<Category>(
            "SELECT * FROM Categories WHERE IsDeleted = 0 ORDER BY DisplayOrder OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Skip = skip, PageSize = pageSize }
        );

        return result.AsList();
    }

    public async Task<IReadOnlyList<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
    {
        var all = await GetAllAsync();
        var result = all.AsQueryable().Where(predicate.Compile()).ToList();
        return result.AsReadOnly();
    }

    // ============ متدهای اختصاصی Category ============

    public async Task<Category?> GetCategoryWithSubCategoriesAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        var categoryDict = new Dictionary<int, Category>();

        var result = await connection.QueryAsync<Category, Category, Category>(
            @"SELECT c.*, sc.*
              FROM Categories c
              LEFT JOIN Categories sc ON c.Id = sc.ParentCategoryId
              WHERE c.Id = @Id AND c.IsDeleted = 0",
            (category, subCategory) =>
            {
                if (!categoryDict.TryGetValue(category.Id, out var currentCategory))
                {
                    currentCategory = category;
                    categoryDict.Add(category.Id, currentCategory);
                    currentCategory.SubCategories = new List<Category>();
                }

                if (subCategory != null && subCategory.Id != category.Id)
                {
                    currentCategory.SubCategories.Add(subCategory);
                }

                return currentCategory;
            },
            new { Id = id },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<Category?> GetCategoryWithProductsAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        var categoryDict = new Dictionary<int, Category>();

        var result = await connection.QueryAsync<Category, Product, Category>(
            @"SELECT c.*, p.*
              FROM Categories c
              LEFT JOIN Products p ON c.Id = p.CategoryId
              WHERE c.Id = @Id AND c.IsDeleted = 0 AND (p.IsDeleted = 0 OR p.Id IS NULL)",
            (category, product) =>
            {
                if (!categoryDict.TryGetValue(category.Id, out var currentCategory))
                {
                    currentCategory = category;
                    categoryDict.Add(category.Id, currentCategory);
                    currentCategory.Products = new List<Product>();
                }

                if (product != null)
                {
                    currentCategory.Products.Add(product);
                }

                return currentCategory;
            },
            new { Id = id },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        using var connection = new SqlConnection(_connectionString);

        if (excludeId.HasValue)
        {
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Categories WHERE Slug = @Slug AND Id != @ExcludeId AND IsDeleted = 0",
                new { Slug = slug, ExcludeId = excludeId.Value }
            );
            return count == 0;
        }
        else
        {
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Categories WHERE Slug = @Slug AND IsDeleted = 0",
                new { Slug = slug }
            );
            return count == 0;
        }
    }

    public async Task<List<Category>> GetMainCategoriesAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Category>(
            "SELECT * FROM Categories WHERE ParentCategoryId IS NULL AND IsActive = 1 AND IsDeleted = 0 ORDER BY DisplayOrder"
        );

        return result.ToList();
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        return await AddAsync(category);
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        await UpdateAsync(category);
        return category;
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Products WHERE CategoryId = @Id AND IsDeleted = 0",
            new { Id = id }
        );

        return count > 0;
    }

    public async Task<bool> HasSubCategoriesAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Categories WHERE ParentCategoryId = @Id AND IsDeleted = 0",
            new { Id = id }
        );

        return count > 0;
    }
}