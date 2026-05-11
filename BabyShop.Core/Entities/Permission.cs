using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;  // اضافه کن

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public Permission() { }

    public Permission(string name, string module, string group, string? description = null)
    {
        Name = name;
        Module = module;
        Group = group;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string module, string group, string? description = null)
    {
        Name = name;
        Module = module;
        Group = group;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}