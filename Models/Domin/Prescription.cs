using PharmacyApi.Models.Domin;

namespace PharmacyApi.Models.Domain;

public class Prescription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FilePath { get; set; } = string.Empty;   // stored path or cloud URL
    public DateTime UploadDate { get; set; }
    public string Status { get; set; } = "Pending";        // Pending, Approved, Rejected
    public string? AdminComments { get; set; }
    public int? OrderId { get; set; }                      // link after order placed

    // Navigation properties (assume User exists from Team A)
    public User? User { get; set; }
    public Order? Order { get; set; }
}