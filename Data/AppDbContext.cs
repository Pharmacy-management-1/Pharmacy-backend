using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models.Domain;
using PharmacyApi.Models.Domin;   // For User, LoyaltyPoint, Product, Category, Inventory
// using PharmacyApi.Models.Domin; // Remove this – it's a typo, use Domain instead

namespace PharmacyApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ==================== Member A ====================
    public DbSet<User> Users { get; set; }
    public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

    // ==================== Member B ====================
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    // ==================== Member C (you) ====================
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // ==================== Member D ====================
    public DbSet<HealthPackage> HealthPackages { get; set; }
    public DbSet<HealthPackageItem> HealthPackageItems { get; set; }
    public DbSet<SeasonalOffer> SeasonalOffers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== Member A: User & LoyaltyPoint ==========
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();

            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Name).HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.Address).HasMaxLength(500);
            entity.Property(u => u.Role).HasDefaultValue("User").HasMaxLength(50);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LoyaltyPoint>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => l.UserId).IsUnique();
            entity.Property(l => l.Points).HasDefaultValue(0);
            entity.Property(l => l.TotalPointsEarned).HasDefaultValue(0);
            entity.Property(l => l.TotalPointsRedeemed).HasDefaultValue(0);
            entity.Property(l => l.Tier).HasDefaultValue("Bronze").HasMaxLength(50);
            entity.Property(l => l.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(l => l.User)
                  .WithOne(u => u.LoyaltyPoint)
                  .HasForeignKey<LoyaltyPoint>(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== Member C: Prescriptions, Orders, OrderItems ==========
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Order)
                  .WithOne(o => o.Prescription)
                  .HasForeignKey<Prescription>(p => p.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(p => p.Status).HasDefaultValue("Pending");
            entity.HasIndex(p => p.Status);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.OrderItems)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.Status).HasDefaultValue("Confirmed");
            entity.Property(o => o.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(o => o.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(oi => oi.OrderId);
        });

        // ========== Member D: HealthPackage, HealthPackageItem, SeasonalOffer ==========
        modelBuilder.Entity<HealthPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountedPrice).HasPrecision(18, 2);

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.HealthPackage)
                  .HasForeignKey(i => i.HealthPackageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HealthPackageItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SeasonalOffer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Ignore(e => e.IsCurrentlyActive);
        });
    }
}