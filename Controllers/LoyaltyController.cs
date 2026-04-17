using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Services;
using System.Security.Claims;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyController(ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("points")]
        public async Task<IActionResult> GetPoints()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized();
            }

            var points = await _loyaltyService.GetPointsAsync(userId.Value);
            return Ok(new { points });
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetLoyaltyInfo()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized();
            }

            var info = await _loyaltyService.GetLoyaltyInfoAsync(userId.Value);

            if (info == null)
            {
                return NotFound(new { message = "Loyalty info not found" });
            }

            return Ok(info);
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}