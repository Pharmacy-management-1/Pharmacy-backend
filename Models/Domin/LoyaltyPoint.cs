using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyApi.Models.Domain
{
    public class LoyaltyPoint
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public int Points { get; set; } = 0;

        public int TotalPointsEarned { get; set; } = 0;

        public int TotalPointsRedeemed { get; set; } = 0;

        public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User? User { get; set; }
    }
}