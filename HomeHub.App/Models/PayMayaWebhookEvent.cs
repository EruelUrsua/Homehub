namespace HomeHub.App.Models
{
    public class PayMayaWebhookEvent
    {
        public string Name { get; set; }
        public string RequestReferenceNumber { get; set; }
        public string PaymentStatus { get; set; }
    }
}
