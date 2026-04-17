using PharmacyApi.Models.Domin;
using System;
using System.Collections.Generic;

namespace PharmacyApi.Models.Domain
{
    public class HealthPackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<HealthPackageItem> Items { get; set; } = new List<HealthPackageItem>();
    }

    public class HealthPackageItem
    {
        public int Id { get; set; }
        public int HealthPackageId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public HealthPackage? HealthPackage { get; set; }
        public Product? Product { get; set; }
    }
}