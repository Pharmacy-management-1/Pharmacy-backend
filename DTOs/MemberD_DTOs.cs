using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmacyApi.Models.DTOs
{
    // ─── Order History DTOs ────────────────────────────────────────────────────
    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemSummaryDto> Items { get; set; } = new();
    }

    public class OrderItemSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // ─── Quick Reorder DTOs ───────────────────────────────────────────────────
    public class QuickReorderRequestDto
    {
        [Required]
        public int OrderId { get; set; }
    }

    public class QuickReorderResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? NewOrderId { get; set; }
        public List<string> OutOfStockItems { get; set; } = new();
    }

    // ─── Health Package DTOs ──────────────────────────────────────────────────
    public class HealthPackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }
        public List<HealthPackageItemDto> Items { get; set; } = new();
    }

    public class HealthPackageItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class CreateHealthPackageDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public decimal? DiscountedPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        [Required, Range(1, 365)]
        public int DurationDays { get; set; }

        public List<HealthPackageItemCreateDto> Items { get; set; } = new();
    }

    public class HealthPackageItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateHealthPackageDto : CreateHealthPackageDto
    {
        public bool IsActive { get; set; } = true;
    }

    // ─── Seasonal Offer DTOs ──────────────────────────────────────────────────
    public class SeasonalOfferDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BannerImageUrl { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public string? CouponCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    public class CreateSeasonalOfferDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string BannerImageUrl { get; set; } = string.Empty;

        [Required, Range(0.01, 100)]
        public decimal DiscountPercentage { get; set; }

        [MaxLength(50)]
        public string? CouponCode { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? CategoryId { get; set; }
    }

    public class UpdateSeasonalOfferDto : CreateSeasonalOfferDto
    {
        public bool IsActive { get; set; } = true;
    }

    // ─── Email DTOs ───────────────────────────────────────────────────────────
    public class SendEmailDto
    {
        [Required, EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;
    }

    public class OrderConfirmationEmailDto
    {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<OrderItemSummaryDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}