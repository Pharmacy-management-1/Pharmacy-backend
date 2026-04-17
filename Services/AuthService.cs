using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmacyApi.Data;
using PharmacyApi.DTOs.Auth;
using PharmacyApi.Models.Domain;
using PharmacyApi.DTOs.Auth;

namespace PharmacyApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILoyaltyService _loyaltyService;

        public AuthService(AppDbContext context, IConfiguration configuration, ILoyaltyService loyaltyService)
        {
            _context = context;
            _configuration = configuration;
            _loyaltyService = loyaltyService;
        }

        public async Task<User?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user exists
            if (await UserExistsAsync(registerDto.Email, registerDto.Username))
            {
                return null;
            }

            var user = new User
            {
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Create loyalty points for the user
            await _loyaltyService.CreateLoyaltyAccountAsync(user.Id);

            return user;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email or username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.UsernameOrEmail ||
                                           u.Username == loginDto.UsernameOrEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            // Get loyalty info
            var loyaltyInfo = await _loyaltyService.GetLoyaltyInfoAsync(user.Id);

            // Update last login
            await UpdateLastLoginAsync(user.Id);

            // Generate tokens (you'll implement JwtHelper)
            var jwtHelper = new Helpers.JwtHelper(_configuration);

            return new LoginResponseDto
            {
                Token = jwtHelper.GenerateJwtToken(user, loyaltyInfo?.Points ?? 0),
                RefreshToken = jwtHelper.GenerateRefreshToken(),
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                LoyaltyPoints = loyaltyInfo?.Points ?? 0,
                LoyaltyTier = loyaltyInfo?.Tier ?? "Bronze"
            };
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.LoyaltyPoint)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var loyaltyInfo = await _loyaltyService.GetLoyaltyInfoAsync(userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                LoyaltyInfo = loyaltyInfo
            };
        }

        public async Task<UserProfileDto?> UpdateUserProfileAsync(int userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.FirstName = updateProfileDto.FirstName;
            user.LastName = updateProfileDto.LastName;
            user.PhoneNumber = updateProfileDto.PhoneNumber;
            user.Address = updateProfileDto.Address;

            await _context.SaveChangesAsync();

            return await GetUserProfileAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserExistsAsync(string email, string username)
        {
            return await _context.Users.AnyAsync(u => u.Email == email || u.Username == username);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}