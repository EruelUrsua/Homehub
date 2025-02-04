namespace HomeHub.App.Models
{
    public class RefundVM
    {
        public int ClientID { get; set; }
        public string BusinessID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderedPs { get; set; }
        public int Quantity { get; set; }
        public string OrderId { get; set; }

        // New properties for fee and promo information
        public string PromoCode { get; set; }
        public decimal OriginalFee { get; set; }
        public decimal Fee { get; set; }
        public decimal DiscountedFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
    }
}