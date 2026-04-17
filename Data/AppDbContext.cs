using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models.Domain;
<<<<<<< Updated upstream
using System.Collections.Generic;
using System.Reflection.Emit;


namespace PharmacyApi.Data
=======
using PharmacyApi.Models.Domin;
using System.Reflection.Emit;

namespace PharmacyApi.Data;

public class AppDbContext : DbContext
>>>>>>> Stashed changes
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Team C: Prescriptions, Orders, OrderItems
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

<<<<<<< Updated upstream
        // Member D DbSets
        public DbSet<HealthPackage> HealthPackages { get; set; }
        public DbSet<HealthPackageItem> HealthPackageItems { get; set; }
        public DbSet<SeasonalOffer> SeasonalOffers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Team C: Prescriptions, Orders, OrderItems
            // Relationships for Prescription
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
=======

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
>>>>>>> Stashed changes

        // Relationships for Prescription
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Prescription)
            .HasForeignKey<Prescription>(p => p.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationships for Order
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship for OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Default values and indexes
        modelBuilder.Entity<Prescription>()
            .Property(p => p.Status)
            .HasDefaultValue("Pending");

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasDefaultValue("Confirmed");

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Prescription>()
            .HasIndex(p => p.Status);

<<<<<<< Updated upstream
            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);


            // Member D DbSets configuration
            // ── HealthPackage ─────────────────────────────
            modelBuilder.Entity<HealthPackage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Price)
                      .HasPrecision(18, 2);

                entity.Property(e => e.DiscountedPrice)
                      .HasPrecision(18, 2);

                entity.HasMany(e => e.Items)
                      .WithOne(i => i.HealthPackage)
                      .HasForeignKey(i => i.HealthPackageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── HealthPackageItem ─────────────────────────
            modelBuilder.Entity<HealthPackageItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── SeasonalOffer ─────────────────────────────
            modelBuilder.Entity<SeasonalOffer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.DiscountPercentage)
                      .HasPrecision(5, 2);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Ignore(e => e.IsCurrentlyActive);
            });
        }
=======
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasIndex(oi => oi.OrderId);
>>>>>>> Stashed changes
    }
}