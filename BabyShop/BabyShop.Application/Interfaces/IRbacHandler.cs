using System.Threading.Tasks;

namespace BabyShop.Application.Interfaces
{
    public interface IRbacHandler
    {
        
        Task<bool> HasPermissionAsync(int userId, string permission);

   
        Task<bool> IsInRoleAsync(int userId, string role);
    }
}