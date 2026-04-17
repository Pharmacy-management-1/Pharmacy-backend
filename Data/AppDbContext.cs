using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ✅ Existing DbSets...

    // ✅ Member D DbSets
    public DbSet<HealthPackage> HealthPackages { get; set; }
    public DbSet<HealthPackageItem> HealthPackageItems { get; set; }
    public DbSet<SeasonalOffer> SeasonalOffers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
}