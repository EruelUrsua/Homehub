using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ProductServiceViewModel
    {
        public string Type { get; set; } // "Product" or "Service"
        public string Name { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        // Product-specific properties
        [Required(ErrorMessage = "Product ID is required")]
        public string ProductId { get; set; }
        [Required(ErrorMessage = "Product Item is required")]
        [StringLength(100, ErrorMessage = "Product Item can't be longer than 100 characters.")]
        public string ProductItem { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public int Qty { get; set; }
        [Required(ErrorMessage = "Container Type is required")]
        public string ContainerType { get; set; }

        // Service-specific properties
        [Required(ErrorMessage = "Service ID is required")]
        public string ServiceId { get; set; }
        [Required(ErrorMessage = "Service Item is required")]
        [StringLength(100, ErrorMessage = "Service Item can't be longer than 100 characters.")]
        public string ServiceItem { get; set; }
        [Required(ErrorMessage = "Details are required")]
        public string Details { get; set; }
        [Required(ErrorMessage = "Fee is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fee must be greater than zero")]
        public decimal Fee { get; set; }
        public bool Available { get; set; }
    }
}
