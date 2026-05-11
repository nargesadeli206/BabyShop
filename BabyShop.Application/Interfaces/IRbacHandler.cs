namespace BabyShop.Application.Interfaces.Services;  

public interface IRbacHandler
{
    Task<bool> HasPermissionAsync(int userId, string permission);
    Task<bool> IsInRoleAsync(int userId, string role);
}