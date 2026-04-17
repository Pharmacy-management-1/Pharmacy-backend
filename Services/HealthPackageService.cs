using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.Models.Domain;
using PharmacyApi.Models.Domin;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public class HealthPackageService : IHealthPackageService
    {
        private readonly AppDbContext _context;

        public HealthPackageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HealthPackageDto>> GetAllAsync()
        {
            var packages = await _context.HealthPackages
                .Include(hp => hp.Items)
                    .ThenInclude(i => i.Product)
                .Where(hp => hp.IsActive)
                .ToListAsync();

            return packages.Select(MapToDto);
        }

        public async Task<HealthPackageDto?> GetByIdAsync(int id)
        {
            var pkg = await _context.HealthPackages
                .Include(hp => hp.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(hp => hp.Id == id);

            return pkg == null ? null : MapToDto(pkg);
        }

        public async Task<HealthPackageDto> CreateAsync(CreateHealthPackageDto dto)
        {
            var pkg = new HealthPackage
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountedPrice = dto.DiscountedPrice,
                ImageUrl = dto.ImageUrl,
                DurationDays = dto.DurationDays,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = dto.Items.Select(i => new HealthPackageItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.HealthPackages.Add(pkg);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return (await GetByIdAsync(pkg.Id))!;
        }

        public async Task<HealthPackageDto?> UpdateAsync(int id, UpdateHealthPackageDto dto)
        {
            var pkg = await _context.HealthPackages
                .Include(hp => hp.Items)
                .FirstOrDefaultAsync(hp => hp.Id == id);

            if (pkg == null) return null;

            pkg.Name = dto.Name;
            pkg.Description = dto.Description;
            pkg.Price = dto.Price;
            pkg.DiscountedPrice = dto.DiscountedPrice;
            pkg.ImageUrl = dto.ImageUrl;
            pkg.DurationDays = dto.DurationDays;
            pkg.IsActive = dto.IsActive;
            pkg.UpdatedAt = DateTime.UtcNow;

            _context.HealthPackageItems.RemoveRange(pkg.Items);

            pkg.Items = dto.Items.Select(i => new HealthPackageItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();

            await _context.SaveChangesAsync();
            return (await GetByIdAsync(pkg.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pkg = await _context.HealthPackages.FindAsync(id);
            if (pkg == null) return false;

            pkg.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        private static HealthPackageDto MapToDto(HealthPackage hp) => new()
        {
            Id = hp.Id,
            Name = hp.Name,
            Description = hp.Description,
            Price = hp.Price,
            DiscountedPrice = hp.DiscountedPrice,
            ImageUrl = hp.ImageUrl,
            DurationDays = hp.DurationDays,
            IsActive = hp.IsActive,
            Items = hp.Items.Select(i => new HealthPackageItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}