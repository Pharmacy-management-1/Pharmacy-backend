using PharmacyApi.DTOs.Auth;


namespace PharmacyApi.Services
{
    public interface ILoyaltyService
    {
        Task<LoyaltyInfoDto?> GetLoyaltyInfoAsync(int userId);
        Task<bool> CreateLoyaltyAccountAsync(int userId);
        Task<bool> AddPointsAsync(int userId, int points, string reason);
        Task<bool> RedeemPointsAsync(int userId, int points);
        Task<int> GetPointsAsync(int userId);
        Task<string> GetTierAsync(int points);
        Task<int> GetPointsToNextTierAsync(int currentPoints);
        Task<string> GetNextTierAsync(int currentPoints);
    }
}