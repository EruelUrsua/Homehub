using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ProductServiceViewModel
    {
        public string Type { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        // Product-specific properties
        [Display(Name = "Product ID")]
        [StringLength(50, ErrorMessage = "Product ID cannot exceed 50 characters.")]
        public string ProductId { get; set; }
        [Display(Name = "Product Item")]

        [StringLength(100, ErrorMessage = "Product Item cannot exceed 50 characters.")]
        public string ProductItem { get; set; }
        [Display(Name = "Quantity")]

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Qty { get; set; }
        [Display(Name = "Container Type")]

        [StringLength(50, ErrorMessage = "Container Type cannot exceed 50 characters.")]
        public string ContainerType { get; set; }

        // Service-specific properties
        [Display(Name = "Service ID")]
        [StringLength(50, ErrorMessage = "Service ID cannot exceed 50 characters.")]
        public string ServiceId { get; set; }
        [Display(Name = "Service Item")]

        [StringLength(100, ErrorMessage = "Service Item cannot exceed 50 characters.")]
        public string ServiceItem { get; set; }

        [StringLength(500, ErrorMessage = "Details cannot exceed 100 characters.")]
        public string Details { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Fee must be a positive number.")]
        public decimal Fee { get; set; }

        public bool Available { get; set; }
    }
}
