using PharmacyApi.Models.Domin;

namespace PharmacyApi.Models.Domain;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Confirmed";      // Confirmed, Shipped, Delivered, Cancelled
    public int? PrescriptionId { get; set; }

    // Navigation
    public User? User { get; set; }
    public Prescription? Prescription { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}