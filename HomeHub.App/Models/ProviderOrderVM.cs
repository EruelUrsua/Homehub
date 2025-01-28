namespace HomeHub.App.Models
{
    public class ProviderOrderVM
    {
        public int ClientId { get; set; }

        public string? BusinessId { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public DateTime Schedule { get; set; }

        public string? OrderedPs { get; set; } = null!;

        public decimal OrigFee { get; set; }
        public decimal Fee { get; set; }
        public decimal DiscountedFee { get; set; }
        public decimal DiscountPercentage { get; set; }

        public bool Status { get; set; }

        public string? PromoCode { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int RatingId { get; set; }

        public int ReportId { get; set; }

        public int Quantity { get; set; }
        public string? ModeOfPayment { get; set; }

        public decimal GetOriginalFee()
        {
            return OrigFee; // Calculates the original fee before discount is applied.
        }
    }
}
