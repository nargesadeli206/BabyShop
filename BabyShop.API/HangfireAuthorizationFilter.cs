using Hangfire.Dashboard;

namespace BabyShop.API;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // در محیط توسعه، همه دسترسی داشته باشند
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        if (env.IsDevelopment())
            return true;

        // در محیط تولید، فقط کاربران احراز هویت شده دسترسی داشته باشند
        var user = httpContext.User;
        return user.Identity?.IsAuthenticated == true;
    }
}