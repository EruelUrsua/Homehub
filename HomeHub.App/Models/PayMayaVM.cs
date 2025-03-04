namespace HomeHub.App.Models
{
    public class PayMayaVM
    {
        public class PayMayaAmount
        {
            public decimal value { get; set; }
            public string currency {  get; set; }
        }

        public class PayMayaItem
        {
            public string name {  get; set; }
            //public decimal amount { get; set; }
            public PayMayaAmount totalAmount { get; set; }
        }

        public class PayMayaRedirectUrl
        {
            public string success { get; set; }
            public string failure { get; set; }
            public string cancel { get; set; }
        }

        public class PayMayaCheckoutRequest
        {
            public PayMayaAmount totalAmount { get; set; }
            public List<PayMayaItem> items { get; set; }
            public string requestReferenceNumber { get; set; }

            public PayMayaRedirectUrl redirectUrl { get; set; }
        }
    }
}
