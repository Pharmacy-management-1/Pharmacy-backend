namespace PharmacyApi.DTOs.Auth
{
    public class LoyaltyInfoDto
    {
        public int Points { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public string Tier { get; set; } = string.Empty;
        public int PointsToNextTier { get; set; }
        public string NextTier { get; set; } = string.Empty;
    }
}