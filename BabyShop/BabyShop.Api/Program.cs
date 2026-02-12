using Microsoft.EntityFrameworkCore;
using BabyShop.BabyShop.Application.Services.Deliveries;
using BabyShop.BabyShop.Application.Services.Inventory;
using BabyShop.BabyShop.Application.Services.Order;
using BabyShop.BabyShop.Application.Services.Products;
using BabyShop.BabyShop.Infrastructure.Data;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.BabyShop.Application.Interfaces;
using BabyShop.BabyShop.Infrastructure.Repositories.Deliveries;
using BabyShop.BabyShop.Infrastructure.Repositories.Inventory;
using BabyShop.BabyShop.Infrastructure.Repositories.Order;
using BabyShop.BabyShop.Infrastructure.Repositories.Products;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Inventory
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Order
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Product
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Delivery
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

//payment
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();