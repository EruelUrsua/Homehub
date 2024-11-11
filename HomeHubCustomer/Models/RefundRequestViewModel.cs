namespace HomeHub.App.Models
{
    public class RefundRequestViewModel
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
    }
}
