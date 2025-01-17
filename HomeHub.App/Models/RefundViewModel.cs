namespace HomeHub.App.Models
{
    public class RefundViewModel
    {
        public int ClientID { get; set; }
        public string BusinessID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderedPs { get; set; }
        public int Quantity { get; set; }
        public string OrderId { get; set; }
    }
}