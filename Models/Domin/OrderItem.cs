using PharmacyApi.Models.Domin;

namespace PharmacyApi.Models.Domain;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public Product? Product { get; set; }   // Product from Team B
}