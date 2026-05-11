using BabyShop.Application.Interfaces;
using BabyShop.Application.Interfaces.Repositories;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Application.Services;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using BabyShop.Infrastructure.Repositories;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BabyShop API",
        Version = "v1",
        Description = "API for BabyShop e-commerce application"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("BabyShop.Infrastructure")));

// ========== Hangfire Configuration ==========
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

// ========== Repositories ==========
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ========== اضافه شده: Basket Repository ==========
builder.Services.AddScoped<IBasketRepository, BasketRepository>();  // فقط یک بار

// ========== Services ==========
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRbacHandler, RbacHandler>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ========== اضافه شده: Basket Services ==========
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<DiscountService, DiscountService>();

// ========== Job Services ==========
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAbandonedCartService, AbandonedCartService>();

var app = builder.Build();

// ========== Middleware ==========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BabyShop API V1");
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ========== Hangfire Dashboard ==========
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "BabyShop Jobs",
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// ========== Register Recurring Jobs ==========
using (var scope = app.Services.CreateScope())
{
    var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate(
        "monthly-report",
        () => jobService.SendMonthlyReportAsync(),
        "0 8 1 * *");

    recurringJobManager.AddOrUpdate(
        "hourly-stock-check",
        () => jobService.CheckLowStockAsync(),
        Cron.Hourly);

    recurringJobManager.AddOrUpdate(
        "weekly-cleanup",
        () => jobService.CleanupOldOrdersAsync(),
        "0 9 * * 5");

    recurringJobManager.AddOrUpdate(
        "test-job",
        () => Console.WriteLine($"✅ Hangfire فعال: {DateTime.Now}"),
        Cron.Minutely);
}

app.Run();

// ========== Hangfire Authorization Filter ==========
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.Host.Host == "localhost" || httpContext.Request.Host.Host == "127.0.0.1")
        {
            return true;
        }

        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}