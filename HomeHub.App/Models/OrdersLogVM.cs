namespace HomeHub.App.Models
{
    public class OrdersLogVM
    {
        public string LogId { get; set; } 
        public string OrderId { get; set; } 
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public int BusinessId { get; set; } 
        public string Item { get; set; } 
        public int Qty { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } 

        // New properties for discount tracking
        public decimal DiscountAmount { get; set; } = 0;
        public double DiscountPercentage { get; set; } = 0;
    }
}
