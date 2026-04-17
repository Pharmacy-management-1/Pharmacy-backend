using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.DTOs;
using PharmacyApi.Helpers;
using PharmacyApi.Services;
using System.Security.Claims;

namespace PharmacyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IWebHostEnvironment _env;

    public PrescriptionsController(IPrescriptionService prescriptionService, IWebHostEnvironment env)
    {
        _prescriptionService = prescriptionService;
        _env = env;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPrescription([FromForm] PrescriptionUploadDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "prescriptions");
        var filePath = await FileUploadHelper.SavePrescriptionFileAsync(dto.File, uploadsFolder);

        var prescription = await _prescriptionService.UploadPrescriptionAsync(userId, filePath, dto.OrderId);
        return Ok(new { prescription.Id, prescription.Status });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyPrescriptions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var prescriptions = await _prescriptionService.GetUserPrescriptionsAsync(userId);
        return Ok(prescriptions);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingPrescriptions()
    {
        var prescriptions = await _prescriptionService.GetPendingPrescriptionsAsync();
        return Ok(prescriptions);
    }

    [HttpPost("approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApprovePrescription([FromBody] PrescriptionApproveDto dto)
    {
        var success = await _prescriptionService.ApprovePrescriptionAsync(dto.PrescriptionId, dto.Approved, dto.Comments);
        if (!success) return NotFound("Prescription not found.");
        return Ok(new { message = dto.Approved ? "Approved" : "Rejected" });
    }
}