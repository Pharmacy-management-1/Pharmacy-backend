using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models.Domin;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string DosageForm { get; set; } = string.Empty; // Tablet, Capsule, Syrup, etc.

    [MaxLength(50)]
    public string? Strength { get; set; } // e.g., "500mg", "10mg/5ml"

    [MaxLength(50)]
    public string? Packaging { get; set; } // e.g., "10 tablets/blister", "100ml bottle"

    [Required]
    public bool RequiresPrescription { get; set; }

    [MaxLength(200)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    public virtual Inventory? Inventory { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

