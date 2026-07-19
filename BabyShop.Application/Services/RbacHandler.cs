using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Interfaces;

namespace BabyShop.Application.Services;

public class RbacHandler : IRbacHandler
{
    private readonly IUserRepository _userRepository;

    public RbacHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var roles = await _userRepository.GetUserRoleNamesAsync(userId);
        if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            return true;

        var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CreateOrder",
            "ViewOwnBasket",
            "ViewOwnOrders",
            "ManageOwnBasket"
        };

        if (userPermissions.Contains(permission)
            && roles.Any(r => r.Equals("User", StringComparison.OrdinalIgnoreCase)
                           || r.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                           || r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            return true;

        if (roles.Any(r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
        {
            var managerPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ManageProducts",
                "ManageInventory",
                "ManageCategories",
                "ViewOrders"
            };
            if (managerPermissions.Contains(permission))
                return true;
        }

        return false;
    }

    public async Task<bool> IsInRoleAsync(int userId, string role)
    {
        var roles = await _userRepository.GetUserRoleNamesAsync(userId);
        return roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}