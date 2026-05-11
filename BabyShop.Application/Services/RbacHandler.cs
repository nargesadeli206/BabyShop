using BabyShop.Application.Interfaces;
using BabyShop.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace BabyShop.Application.Services
{
    public class RbacHandler : IRbacHandler
    {
        // اینجا می‌تونی بعداً Repository کاربر رو تزریق کنی
        public RbacHandler()
        {
        }

        // بررسی می‌کنه کاربر اجازه داره یا نه
        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            // اینجا باید بری تو دیتابیس ببینی این کاربر این اجازه رو داره یا نه
            // فعلاً برای اینکه پروژه ران بشه، همه رو مجاز می‌ذاریم
            // بعداً می‌تونی منطق واقعی رو اینجا بنویسی

            // مثال ساده:
            // اگه userId = 1 بود یعنی مدیر کل، همه کارها مجازه
            if (userId == 1)
                return await Task.FromResult(true);

            // اگه permission = "CreateOrder" بود و کاربر عادی بود، اجازه بده
            if (permission == "CreateOrder")
                return await Task.FromResult(true);

            // بقیه موارد رو فعلاً مجاز می‌ذاریم تا پروژه کار کنه
            return await Task.FromResult(true);
        }

        // بررسی می‌کنه کاربر در چه نقشی هست
        public async Task<bool> IsInRoleAsync(int userId, string role)
        {
            // اینجا باید بری تو دیتابیس ببینی این کاربر این نقش رو داره یا نه
            // فعلاً برای اینکه پروژه ران بشه، همه رو مجاز می‌ذاریم

            // مثال: کاربر 1 مدیر کل هست
            if (userId == 1 && role == "Admin")
                return await Task.FromResult(true);

            // بقیه کاربران نقش Customer دارند
            if (role == "Customer")
                return await Task.FromResult(true);

            return await Task.FromResult(true);
        }
    }
}