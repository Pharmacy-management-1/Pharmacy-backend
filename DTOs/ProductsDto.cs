using System.ComponentModel.DataAnnotations;

namespace PharmacyApi.DTOs;

public class ProductCreateDTO
{
    
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Required]
        public string DosageForm { get; set; } = string.Empty;

        public string? Strength { get; set; }

        public string? Packaging { get; set; }

        public bool RequiresPrescription { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Inventory fields
        [Required]
        [Range(0, 100000)]
        public int InitialStock { get; set; }

        [Required]
        [Range(0, 100000)]
        public int ReorderLevel { get; set; }

        [Required]
        [Range(0, 100000)]
        public int ReorderQuantity { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal CostPrice { get; set; }

        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string? Location { get; set; }
    }

    public class ProductUpdateDTO
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [Range(0, 1000000)]
        public decimal? Price { get; set; }

        public string? DosageForm { get; set; }

        public string? Strength { get; set; }

        public string? Packaging { get; set; }

        public bool? RequiresPrescription { get; set; }

        public string? ImageUrl { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string DosageForm { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string? Packaging { get; set; }
        public bool RequiresPrescription { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductFilterDTO
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public string? DosageForm { get; set; }
        public string? Packaging { get; set; }
        public string? Manufacturer { get; set; }
        public bool? RequiresPrescription { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } // Name, Price, CreatedAt
        public string? SortOrder { get; set; } // ASC, DESC
    }

    public class InventoryUpdateDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int QuantityChange { get; set; } // Positive for addition, negative for removal

        public string? Reason { get; set; } // Sale, Restock, Return, Damaged

        public string? ReferenceNumber { get; set; } // Order ID, Purchase Order ID
    }

    public class InventoryResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool NeedsReorder { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpiringSoon { get; set; }
        public bool IsExpired { get; set; }
        public DateTime LastUpdated { get; set; }
    }
