using Microsoft.EntityFrameworkCore;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Auth;
using PharmacyApi.Models.Domain;

namespace PharmacyApi.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly AppDbContext _context;

        // Tier thresholds
        private readonly Dictionary<string, int> _tierThresholds = new()
        {
            { "Bronze", 0 },
            { "Silver", 100 },
            { "Gold", 500 },
            { "Platinum", 1000 }
        };

        public LoyaltyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LoyaltyInfoDto?> GetLoyaltyInfoAsync(int userId)
        {
            var loyalty = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty == null) return null;

            var currentPoints = loyalty.Points;
            var nextTier = await GetNextTierAsync(currentPoints);
            var pointsToNextTier = await GetPointsToNextTierAsync(currentPoints);

            return new LoyaltyInfoDto
            {
                Points = loyalty.Points,
                TotalPointsEarned = loyalty.TotalPointsEarned,
                TotalPointsRedeemed = loyalty.TotalPointsRedeemed,
                Tier = loyalty.Tier,
                PointsToNextTier = pointsToNextTier,
                NextTier = nextTier
            };
        }

        public async Task<bool> CreateLoyaltyAccountAsync(int userId)
        {
            var existing = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (existing != null) return false;

            var loyalty = new LoyaltyPoint
            {
                UserId = userId,
                Points = 0,
                TotalPointsEarned = 0,
                TotalPointsRedeemed = 0,
                Tier = "Bronze",
                LastUpdatedAt = DateTime.UtcNow
            };

            await _context.LoyaltyPoints.AddAsync(loyalty);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddPointsAsync(int userId, int points, string reason)
        {
            var loyalty = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty == null) return false;

            loyalty.Points += points;
            loyalty.TotalPointsEarned += points;
            loyalty.LastUpdatedAt = DateTime.UtcNow;

            // Update tier based on total points earned
            loyalty.Tier = await GetTierAsync(loyalty.TotalPointsEarned);

            await _context.SaveChangesAsync();

            // Log point transaction (optional - create a PointsTransaction table)
            return true;
        }

        public async Task<bool> RedeemPointsAsync(int userId, int points)
        {
            var loyalty = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (loyalty == null || loyalty.Points < points) return false;

            loyalty.Points -= points;
            loyalty.TotalPointsRedeemed += points;
            loyalty.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPointsAsync(int userId)
        {
            var loyalty = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            return loyalty?.Points ?? 0;
        }

        public async Task<string> GetTierAsync(int points)
        {
            return await Task.Run(() =>
            {
                if (points >= 1000) return "Platinum";
                if (points >= 500) return "Gold";
                if (points >= 100) return "Silver";
                return "Bronze";
            });
        }

        public async Task<int> GetPointsToNextTierAsync(int currentPoints)
        {
            return await Task.Run(() =>
            {
                if (currentPoints < 100) return 100 - currentPoints;
                if (currentPoints < 500) return 500 - currentPoints;
                if (currentPoints < 1000) return 1000 - currentPoints;
                return 0;
            });
        }

        public async Task<string> GetNextTierAsync(int currentPoints)
        {
            return await Task.Run(() =>
            {
                if (currentPoints < 100) return "Silver";
                if (currentPoints < 500) return "Gold";
                if (currentPoints < 1000) return "Platinum";
                return "Max";
            });
        }
    }
}