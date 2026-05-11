using BabyShop.Core.Entities;
using System.Linq.Expressions;

namespace BabyShop.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    // متدهای اختصاصی Role
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetRoleWithPermissionsAsync(int roleId);
    Task<IReadOnlyList<Role>> GetAllAsync();
    Task<Role> AddAsync(Role entity);
    Task UpdateAsync(Role entity);
    Task DeleteAsync(Role entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<IReadOnlyList<Role>> GetPagedAsync(int page, int pageSize);
    Task<Role?> FirstOrDefaultAsync(Expression<Func<Role, bool>> predicate);
    Task<IReadOnlyList<Role>> FindAsync(Expression<Func<Role, bool>> predicate);

    // متدهای اضافی برای مدیریت دسترسی‌ها
    Task AddPermissionToRoleAsync(int roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
}