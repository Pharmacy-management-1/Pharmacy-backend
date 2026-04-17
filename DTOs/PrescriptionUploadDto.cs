using Microsoft.AspNetCore.Http;

namespace PharmacyApi.DTOs;

public class PrescriptionUploadDto
{
    public IFormFile File { get; set; } = null!;
    public int? OrderId { get; set; }   // optional, if uploaded during checkout
}