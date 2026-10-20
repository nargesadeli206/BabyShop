using BabyShop.Core.Entities;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.API;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // ----------------- حل مشکل اصولی AgeRange و Index قدیمی -----------------
        // ۱. ساختن ستون AgeRange (اگه نباشه)
        var addColumnSql = @"
            IF COL_LENGTH('Inventories', 'AgeRange') IS NULL
            BEGIN
                ALTER TABLE Inventories
                ADD AgeRange NVARCHAR(50) NULL;
            END
        ";
        await context.Database.ExecuteSqlRawAsync(addColumnSql);

        // ۲. حذف ایندکس قدیمیِ مزاحم که نمی‌ذاره چند تا سایز ثبت بشه (حل ارور IX_Inventories_ProductId)
        var dropIndexSql = @"
            IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Inventories_ProductId')
            BEGIN
                DROP INDEX IX_Inventories_ProductId ON Inventories;
            END
        ";
        await context.Database.ExecuteSqlRawAsync(dropIndexSql);

        // ۳. ساخت یک ایندکس جدید (ترکیب محصول + رده سنی نباید تکراری بشه، نه فقط محصول)
        var createNewIndexSql = @"
            IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Inventories_Product_AgeRange')
            BEGIN
                CREATE UNIQUE NONCLUSTERED INDEX IX_Inventories_Product_AgeRange 
                ON Inventories(ProductId, AgeRange) 
                WHERE IsDeleted = 0 AND AgeRange IS NOT NULL;
            END
        ";
        await context.Database.ExecuteSqlRawAsync(createNewIndexSql);
        // --------------------------------------------------------------------------

        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role("Admin", "مدیر سیستم"),
                new Role("Manager", "مدیر فروشگاه"),
                new Role("User", "کاربر عادی")
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(u => u.PhoneNumber == "09123456789"))
        {
            var adminUser = new User("مدیر سیستم", "09123456789", passwordHasher.HashPassword("Admin@123"));
            adminUser.VerifyPhone();

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var userRole = new UserRole(adminUser.Id, adminRole.Id);
                context.UserRoles.Add(userRole);
                await context.SaveChangesAsync();
            }
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category("پوشاک", "انواع لباس کودک", null, null, 1),
                new Category("اسباب بازی", "انواع اسباب بازی", null, null, 2),
                new Category("لوازم بهداشتی", "لوازم بهداشتی کودک", null, null, 3),
                new Category("کتاب و لوازم تحریر", "کتاب و لوازم تحریر کودک", null, null, 4),
                new Category("تغذیه", "لوازم تغذیه کودک", null, null, 5),
                new Category("کالسکه و صندلی ماشین", "کالسکه و صندلی ماشین کودک", null, null, 6),
                new Category("تجهیزات اتاق کودک", "تجهیزات اتاق کودک", null, null, 7)
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}