namespace HomeHub.App.Models
{
    public class ProductServiceViewModel
    {
        public string Type { get; set; } // "Product" or "Service"
        public string Name { get; set; }
        public decimal Price { get; set; }

        // Product-specific properties
        public string ProductId { get; set; }
        public string ProductItem { get; set; }
        public int Qty { get; set; }
        public string ContainerType { get; set; }

        // Service-specific properties
        public string ServiceId { get; set; }
        public string ServiceItem { get; set; }
        public string Details { get; set; }
        public decimal Fee { get; set; }
        public bool Available { get; set; }
    }
}
