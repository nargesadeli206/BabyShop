using BabyShop.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // این کانستراکتور اضافه می‌شود تا اگر EF نتواند کانکشن استرینگ را از StartupProject بگیرد، مستقیم وصل شود
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // کانکشن استرینگ خودت را اینجا وارد کن
            optionsBuilder.UseSqlServer("Server=.;Database=BabyShopDb;Trusted_Connection=True;");
        }
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete filters
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Inventory>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Delivery>().HasQueryFilter(d => !d.IsDeleted);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Description).IsRequired().HasMaxLength(1000);
            entity.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(p => p.CategoryId).IsRequired();
            entity.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(p => p.ViewCount).IsRequired().HasDefaultValue(0);
            entity.Property(p => p.SoldCount).IsRequired().HasDefaultValue(0);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.Property(c => c.DisplayOrder).IsRequired().HasDefaultValue(0);
            entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventory
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductId).IsRequired();
            entity.Property(i => i.CurrentStock).IsRequired().HasDefaultValue(0);
            entity.Property(i => i.ReservedStock).IsRequired().HasDefaultValue(0);
            entity.Property(i => i.MinimumStockLevel).IsRequired().HasDefaultValue(5);
            entity.Property(i => i.MaximumStockLevel).IsRequired().HasDefaultValue(1000);
            entity.Property(i => i.ReorderPoint).IsRequired().HasDefaultValue(10);
            entity.Property(i => i.Location).HasMaxLength(100).HasDefaultValue("Main Warehouse");

            entity.HasOne(i => i.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
            entity.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(15);
            entity.Property(o => o.Email).HasMaxLength(100);
            entity.Property(o => o.Status).IsRequired().HasMaxLength(20);
            entity.Property(o => o.SubTotal).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.ShippingCost).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.Tax).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.Discount).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.TotalAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(o => o.CustomerNote).HasMaxLength(500);
            entity.Property(o => o.AdminNote).HasMaxLength(500);
            entity.Property(o => o.CancellationReason).HasMaxLength(500);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 2);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OrderId).IsRequired();
            entity.Property(p => p.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(p => p.Authority).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Authority).IsUnique();
            entity.Property(p => p.Status).IsRequired().HasMaxLength(20);
            entity.Property(p => p.TransactionId).HasMaxLength(100);

            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Delivery
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.OrderId).IsRequired();
            entity.Property(d => d.Address).IsRequired().HasMaxLength(500);
            entity.Property(d => d.PhoneNumber).IsRequired().HasMaxLength(15);
            entity.Property(d => d.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(d => d.Status).IsRequired().HasMaxLength(20);
            entity.Property(d => d.TrackingNumber).HasMaxLength(50);
            entity.Property(d => d.Carrier).HasMaxLength(50);

            entity.HasOne(d => d.Order)
                .WithOne(o => o.Delivery)
                .HasForeignKey<Delivery>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
