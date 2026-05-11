using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;

namespace BabyShop.Application.Interfaces.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name);
    Task<List<Permission>> GetPermissionsByRoleIdAsync(int roleId);
}