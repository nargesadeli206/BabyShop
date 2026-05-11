using BabyShop.Core.Entities;
using BabyShop.Core.Entities.Base;
using BabyShop.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ============ DbSet ها (جدول‌های دیتابیس) ============
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============ Soft Delete Filters ============
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(ur => !ur.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(rp => !rp.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Inventory>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => !oi.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Delivery>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Basket>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<BasketItem>().HasQueryFilter(bi => !bi.IsDeleted);

        // ============ User Configuration ============
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(11);
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.VerificationCode).HasMaxLength(6);
            entity.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ Role Configuration ============
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Description).HasMaxLength(200);
        });

        // ============ UserRole Configuration ============
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);
            entity.Property(ur => ur.UserId).IsRequired();
            entity.Property(ur => ur.RoleId).IsRequired();
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============ Permission Configuration ============
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Name).IsUnique();
            entity.Property(p => p.Module).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(200);
        });

        // ============ RolePermission Configuration ============
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => rp.Id);
            entity.Property(rp => rp.RoleId).IsRequired();
            entity.Property(rp => rp.PermissionId).IsRequired();
            entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============ Product Configuration ============
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(p => p.CategoryId).IsRequired();

            // Value Object conversions
            entity.Property(p => p.Gender)
                .HasConversion(
                    v => v.Value,
                    v => Gender.FromValue(v)
                );

            entity.Property(p => p.AgeRange)
                .HasConversion(
                    v => v.Code,
                    v => AgeRange.FromCode(v)
                )
                .HasMaxLength(10);

            entity.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(p => p.ViewCount).IsRequired().HasDefaultValue(0);
            entity.Property(p => p.SoldCount).IsRequired().HasDefaultValue(0);
            entity.Property(p => p.MainImage).HasMaxLength(500);
            entity.Property(p => p.ImageUrl).HasMaxLength(500);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.Gender);
            entity.HasIndex(p => p.AgeRange);
            entity.HasIndex(p => new { p.Gender, p.AgeRange });
        });

        // ============ Category Configuration ============
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

        // ============ Inventory Configuration ============
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

        // ============ Order Configuration ============
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

        // ============ OrderItem Configuration ============
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

        // ============ Payment Configuration ============
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.OrderId).IsRequired();
            entity.Property(p => p.Amount).IsRequired().HasPrecision(18, 2);
            entity.Property(p => p.Status).IsRequired().HasMaxLength(20);
            entity.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Authority).HasMaxLength(50);
            entity.HasIndex(p => p.Authority).IsUnique().HasFilter("[Authority] IS NOT NULL");
            entity.Property(p => p.ReferenceNumber).HasMaxLength(100);
            entity.Property(p => p.RefundReason).HasMaxLength(500);
            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============ Delivery Configuration ============
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

        // ============ Basket Configuration ============
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.UserId).IsRequired();
            entity.Property(b => b.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(b => b.TotalPrice).IsRequired().HasPrecision(18, 2).HasDefaultValue(0);
            entity.Property(b => b.DiscountCode).HasMaxLength(50);
            entity.Property(b => b.DiscountAmount).HasPrecision(18, 2);
            entity.Property(b => b.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(b => b.SessionId).HasMaxLength(100);
            entity.Property(b => b.IpAddress).HasMaxLength(45);
            entity.Property(b => b.UserAgent).HasMaxLength(500);

            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.SessionId);
            entity.HasIndex(b => b.Status);
            entity.HasIndex(b => b.CreatedAt);

            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(b => b.Items)
                .WithOne(i => i.Basket)
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============ BasketItem Configuration ============
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.HasKey(bi => bi.Id);
            entity.Property(bi => bi.Quantity).IsRequired();
            entity.Property(bi => bi.UnitPrice).IsRequired().HasPrecision(18, 2);
            entity.Property(bi => bi.DiscountPerUnit).HasPrecision(18, 2);

            entity.HasIndex(bi => bi.BasketId);
            entity.HasIndex(bi => bi.ProductId);
            entity.HasIndex(bi => new { bi.BasketId, bi.ProductId }).IsUnique();

            entity.HasOne(bi => bi.Product)
                .WithMany()
                .HasForeignKey(bi => bi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

    // ============ Soft Delete ============
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