
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models.Domin;

public class Inventory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }
    [Required]
    public int ReorderLevel { get; set; }
    [Required]
    public int ReorderQuantity { get; set; }
    [Required]
    public int StockQuantity { get; set; }

    public int ReservedQuantity { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    [Required]
    public decimal CostPrice { get; set; }

    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Location { get; set; }

    // Navigation
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}