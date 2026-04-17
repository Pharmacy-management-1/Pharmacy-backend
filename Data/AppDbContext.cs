using Microsoft.EntityFrameworkCore;
using PharmacyApi.Models.Domain;
using System.Reflection.Emit;

namespace PharmacyApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Team C: Prescriptions, Orders, OrderItems
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId);
        }
    }
}