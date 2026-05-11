using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BabyShop.Infrastructure.Data
{
    // این کلاس برای EF Core در زمان ساخت Migrations و Update-Database استفاده می‌شود
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // کانکشن استرینگ مستقیم با TrustServerCertificate=True برای رفع خطای SSL
            optionsBuilder.UseSqlServer(
                "Server=.;Database=BabyShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
