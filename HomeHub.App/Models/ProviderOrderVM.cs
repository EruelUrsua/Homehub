namespace HomeHub.App.Models
{
    public class ProviderOrderVM
    {
        public int ClientId { get; set; }

        public string? BusinessId { get; set; } 

        public DateTime OrderDate { get; set; }

        public DateTime Schedule { get; set; }

        public string? OrderedPs { get; set; } = null!;
        public decimal OriginalFee { get; set; }
        public decimal Fee { get; set; }
        public decimal DiscountedFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }

        public string Status { get; set; }//pakicheck to

        public string? PromoCode { get; set; } = string.Empty;

        public string UserId { get; set; }

        public int RatingId { get; set; }

        public int ReportId { get; set; }

        public int Quantity { get; set; }
        public string? ModeOfPayment { get; set; }
    }
}
