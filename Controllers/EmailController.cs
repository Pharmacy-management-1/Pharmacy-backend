using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models.DTOs;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>Send a custom email (Admin only). Used for notifications/announcements.</summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _emailService.SendEmailAsync(dto.To, dto.Subject, dto.Body);
            return Ok(new { message = $"Email sent to {dto.To}." });
        }
    }
}