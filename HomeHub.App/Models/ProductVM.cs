using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ProductVM
    {
        public string ProductId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Product name can't be longer than 50 characters.")]
        public string ProductItem { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Qty { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Container type can't be longer than 50 characters.")]
        public string ContainerType { get; set; }

        public int ProviderID { get; set; }  // Link to business provider
    }
}
