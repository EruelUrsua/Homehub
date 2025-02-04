namespace HomeHub.App.Models
{
    public class RefundRequestVM
    {
        public string LogId { get; set; }
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessId { get; set; }
        public string Item { get; set; }
        public int Qty { get; set; }
        public DateTime Date { get; set; } // Date of refund request
        public string Status { get; set; } // Status should be "Refund Requested"
        public string PromoCode { get; set; }
        public decimal OriginalFee { get; set; }
        public decimal Fee { get; set; }
        public decimal DiscountedFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }

    }
}
