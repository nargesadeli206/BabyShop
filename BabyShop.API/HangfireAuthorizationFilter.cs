using Hangfire.Dashboard;

namespace BabyShop.API;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();

        if (env?.IsDevelopment() == true)
        {
            var remote = httpContext.Connection.RemoteIpAddress;
            if (remote == null)
                return true;

            return System.Net.IPAddress.IsLoopback(remote)
                   || remote.Equals(httpContext.Connection.LocalIpAddress);
        }

        var user = httpContext.User;
        return user.Identity?.IsAuthenticated == true
               && user.IsInRole("Admin");
    }
}