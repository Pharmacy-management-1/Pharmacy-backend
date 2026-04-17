using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.Models.Domain;
using PharmacyApi.Models.DTOs;

namespace PharmacyApi.Services
{
    public class OfferService : IOfferService
    {
        private readonly AppDbContext _context;

        public OfferService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SeasonalOfferDto>> GetAllOffersAsync()
        {
            var offers = await _context.SeasonalOffers
                .Include(o => o.Category)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return offers.Select(MapToDto);
        }

        public async Task<IEnumerable<SeasonalOfferDto>> GetActiveOffersAsync()
        {
            var now = DateTime.UtcNow;
            var offers = await _context.SeasonalOffers
                .Include(o => o.Category)
                .Where(o => o.IsActive && o.StartDate <= now && o.EndDate >= now)
                .OrderByDescending(o => o.DiscountPercentage)
                .ToListAsync();
            return offers.Select(MapToDto);
        }

        public async Task<SeasonalOfferDto?> GetOfferByIdAsync(int id)
        {
            var offer = await _context.SeasonalOffers
                .Include(o => o.Category)
                .FirstOrDefaultAsync(o => o.Id == id);
            return offer == null ? null : MapToDto(offer);
        }

        public async Task<SeasonalOfferDto> CreateOfferAsync(CreateSeasonalOfferDto dto)
        {
            var offer = new SeasonalOffer
            {
                Title = dto.Title,
                Description = dto.Description,
                BannerImageUrl = dto.BannerImageUrl,
                DiscountPercentage = dto.DiscountPercentage,
                CouponCode = dto.CouponCode,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SeasonalOffers.Add(offer);
            await _context.SaveChangesAsync();
            return (await GetOfferByIdAsync(offer.Id))!;
        }

        public async Task<SeasonalOfferDto?> UpdateOfferAsync(int id, UpdateSeasonalOfferDto dto)
        {
            var offer = await _context.SeasonalOffers.FindAsync(id);
            if (offer == null) return null;

            offer.Title = dto.Title;
            offer.Description = dto.Description;
            offer.BannerImageUrl = dto.BannerImageUrl;
            offer.DiscountPercentage = dto.DiscountPercentage;
            offer.CouponCode = dto.CouponCode;
            offer.StartDate = dto.StartDate;
            offer.EndDate = dto.EndDate;
            offer.CategoryId = dto.CategoryId;
            offer.IsActive = dto.IsActive;
            offer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (await GetOfferByIdAsync(offer.Id))!;
        }

        public async Task<bool> DeleteOfferAsync(int id)
        {
            var offer = await _context.SeasonalOffers.FindAsync(id);
            if (offer == null) return false;

            offer.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static SeasonalOfferDto MapToDto(SeasonalOffer o) => new()
        {
            Id = o.Id,
            Title = o.Title,
            Description = o.Description,
            BannerImageUrl = o.BannerImageUrl,
            DiscountPercentage = o.DiscountPercentage,
            CouponCode = o.CouponCode,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            IsActive = o.IsActive,
            CategoryId = o.CategoryId,
            IsCurrentlyActive = o.IsCurrentlyActive
        };
    }
}