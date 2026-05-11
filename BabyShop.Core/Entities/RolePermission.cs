using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;

    public RolePermission() { }

    public RolePermission(int roleId, int permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        CreatedAt = DateTime.UtcNow;
    }
}