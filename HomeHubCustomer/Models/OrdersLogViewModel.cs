namespace HomeHub.App.Models
{
    public class OrdersLogViewModel
    {
        public string LogId { get; set; } = null!;

        public string OrderId { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string BusinessId { get; set; } = null!;

        public string Item { get; set; } = null!;

        public int Qty { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; } = null!;
    }
}
