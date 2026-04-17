using PharmacyApi.DTOs.Auth;
using PharmacyApi.Models.Domain;
using PharmacyApi.DTOs.Auth;

namespace PharmacyApi.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<UserProfileDto?> UpdateUserProfileAsync(int userId, UpdateProfileDto updateProfileDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> UserExistsAsync(string email, string username);
        Task<User?> GetUserByIdAsync(int userId);
    }
}