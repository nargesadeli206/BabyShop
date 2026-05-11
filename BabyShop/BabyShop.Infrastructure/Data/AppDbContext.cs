using BabyShop.Core.Entities;
using BabyShop.Core.Entities.Base;
using BabyShop.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BabyShop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet ها
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Soft delete filters
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Inventory>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Delivery>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Basket>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<BasketItem>().HasQueryFilter(bi => !bi.IsDeleted);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(11);
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("Email IS NOT NULL");
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.VerificationCode).HasMaxLength(6);
            entity.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(200);
            entity.HasIndex(p => p.Slug).IsUnique();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(p => p.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(p => p.CategoryId)
                .IsRequired();

            entity.Property(p => p.Gender)
                .HasConversion(
                    v => v.Value,
                    v => Gender.FromInt(v)
                );

            entity.Property(p => p.AgeRange)
                .HasConversion(
                    v => v.Code,
                    v => AgeRange.FromCode(v)
                )
                .HasMaxLength(10);

            entity.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(p => p.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(p => p.SoldCount)
                .IsRequired()
                .HasDefaultValue(0);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.Gender);
            entity.HasIndex(p => p.AgeRange);
            entity.HasIndex(p => new { p.Gender, p.AgeRange });
        });

        // Category Configuration
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

        // Inventory Configuration
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

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.Property(o => o.UserId).IsRequired();
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
            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 2);
            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
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

        // Delivery Configuration
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

        // ========== Basket Configuration ==========
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.Property(e => e.DiscountCode)
                .HasMaxLength(50);

            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountPercentage)
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.SessionId)
                .HasMaxLength(100);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            // Index ها
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.CreatedAt);

            // رابطه با User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // رابطه با آیتم‌های سبد خرید
            entity.HasMany(e => e.Items)
                .WithOne(e => e.Basket)
                .HasForeignKey(e => e.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== BasketItem Configuration ==========
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountPerUnit)
                .HasColumnType("decimal(18,2)");

            // Index ها
            entity.HasIndex(e => e.BasketId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.BasketId, e.ProductId }).IsUnique();

            // رابطه با Product
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}