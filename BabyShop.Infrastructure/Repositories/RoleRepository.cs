using BabyShop.Application.Interfaces.Repositories;
using BabyShop.Core.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq.Expressions;

namespace BabyShop.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly string _connectionString;

    public RoleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Role>(
            "SELECT * FROM Roles WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id }
        );
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Role>(
            "GetRoleByName",
            new { Name = name },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        using var connection = new SqlConnection(_connectionString);

        var role = await GetByIdAsync(roleId);
        if (role == null) return null;

        var permissions = await connection.QueryAsync<Permission>(
            @"SELECT p.* FROM Permissions p
              INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
              WHERE rp.RoleId = @RoleId AND p.IsDeleted = 0",
            new { RoleId = roleId }
        );

        role.RolePermissions = permissions.Select(p => new RolePermission
        {
            RoleId = roleId,
            PermissionId = p.Id,
            Permission = p
        }).ToList();

        return role;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var result = await connection.QueryAsync<Role>(
            "GetAllRoles",
            commandType: CommandType.StoredProcedure
        );

        return result.AsList();
    }

    public async Task<Role> AddAsync(Role entity)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@Name", entity.Name);
        parameters.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "InsertRole",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        entity.Id = parameters.Get<int>("@Id");
        return entity;
    }

    public async Task UpdateAsync(Role entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "UpdateRole",
            new { Id = entity.Id, Name = entity.Name },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAsync(Role entity)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "DeleteRole",
            new { Id = entity.Id },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var role = await GetByIdAsync(id);
        return role != null;
    }

    public async Task<int> CountAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Roles WHERE IsDeleted = 0"
        );
    }

    public async Task<IReadOnlyList<Role>> GetPagedAsync(int page, int pageSize)
    {
        using var connection = new SqlConnection(_connectionString);

        var skip = (page - 1) * pageSize;

        var result = await connection.QueryAsync<Role>(
            "SELECT * FROM Roles WHERE IsDeleted = 0 ORDER BY Id OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Skip = skip, PageSize = pageSize }
        );

        return result.AsList();
    }

    public async Task<Role?> FirstOrDefaultAsync(Expression<Func<Role, bool>> predicate)
    {
        var all = await GetAllAsync();
        return all.AsQueryable().FirstOrDefault(predicate.Compile());
    }

    public async Task<IReadOnlyList<Role>> FindAsync(Expression<Func<Role, bool>> predicate)
    {
        var all = await GetAllAsync();
        var result = all.AsQueryable().Where(predicate.Compile()).ToList();
        return result.AsReadOnly();
    }

    public async Task AddPermissionToRoleAsync(int roleId, int permissionId)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@RoleId, @PermissionId)",
            new { RoleId = roleId, PermissionId = permissionId }
        );
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "DELETE FROM RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId",
            new { RoleId = roleId, PermissionId = permissionId }
        );
    }
}