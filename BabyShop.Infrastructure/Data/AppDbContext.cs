using BabyShop.Core.Entities;
using BabyShop.Core.Entities.Base;
using BabyShop.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BabyShop.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Query Filters
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
            modelBuilder.Entity<SupportTicket>().HasQueryFilter(st => !st.IsDeleted);
            modelBuilder.Entity<TicketReply>().HasQueryFilter(tr => !tr.IsDeleted);

            // User Configuration
            modelBuilder.Entity<User>(entity => {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(11);
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
                entity.HasMany(u => u.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            // Product Configuration (حل مشکل EF Core با AgeRange و Gender)
            modelBuilder.Entity<Product>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                // تبدیل AgeRange به استرینگ در دیتابیس
                entity.Property(p => p.AgeRange)
                    .HasConversion(
                        v => v.Code, // اینجا Code که خودش استرینگه ذخیره می‌شه
                        v => AgeRange.FromCode(v)
                    )
                    .HasMaxLength(50)
                    .IsRequired(false);

                // تبدیل Gender به استرینگ در دیتابیس (مشکل int to string اینجا حل شد)
                entity.Property(p => p.Gender)
                    .HasConversion(
                        v => v.Value.ToString(), // اینجا حتماً به استرینگ کست می‌شه
                        v => Gender.FromAny(v)
                    )
                    .HasMaxLength(50)
                    .IsRequired(false);
            });

            // Inventory Configuration
            modelBuilder.Entity<Inventory>(entity => {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.Product)
                      .WithMany()
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity => {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
                entity.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(o => o.Status).IsRequired().HasMaxLength(20);
                entity.Property(o => o.SubTotal).IsRequired().HasPrecision(18, 2);
                entity.Property(o => o.ShippingCost).IsRequired().HasPrecision(18, 2);
                entity.Property(o => o.Tax).IsRequired().HasPrecision(18, 2);
                entity.Property(o => o.Discount).IsRequired().HasPrecision(18, 2);
                entity.Property(o => o.TotalAmount).IsRequired().HasPrecision(18, 2);
                entity.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity => {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 2);
                entity.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).IsRequired().HasPrecision(18, 2);
                entity.HasOne(p => p.Order).WithOne(o => o.Payment).HasForeignKey<Payment>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // Delivery Configuration
            modelBuilder.Entity<Delivery>(entity => {
                entity.HasKey(d => d.Id);
                entity.HasOne(d => d.Order).WithOne(o => o.Delivery).HasForeignKey<Delivery>(d => d.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(cancellationToken);
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
}