using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models.Domin;

public class Inventory
{
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int QuantityInStock { get; set; }

        public int ReservedQuantity { get; set; } = 0;

        [Required]
        public int ReorderLevel { get; set; }

        [Required]
        public int ReorderQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? Location { get; set; } // Warehouse shelf location
    }

