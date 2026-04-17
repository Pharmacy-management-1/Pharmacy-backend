namespace PharmacyApi.DTOs;

public class PrescriptionApproveDto
{
    public int PrescriptionId { get; set; }
    public bool Approved { get; set; }
    public string? Comments { get; set; }
}