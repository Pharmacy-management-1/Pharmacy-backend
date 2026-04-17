using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models.DTOs;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuickReorderController : ControllerBase
    {
        private readonly IQuickReorderService _quickReorderService;

        public QuickReorderController(IQuickReorderService quickReorderService)
        {
            _quickReorderService = quickReorderService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Reorder items from a previous order.</summary>
        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] QuickReorderRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _quickReorderService.ReorderAsync(request.OrderId, GetUserId());

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}