using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ProductVM
    {
        [Required]
        [Display(Name = "Product ID")]
        public string ProductId { get; set; }

        [Required]
        [Display(Name = "Product Item")]
        [StringLength(50, ErrorMessage = "Product name can't be longer than 50 characters.")]
        public string ProductItem { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Qty { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Container Type")]
        [StringLength(50, ErrorMessage = "Container type can't be longer than 50 characters.")]
        public string ContainerType { get; set; }

        public int ProviderID { get; set; }  // Link to business provider
        public IFormFile ProductImage { get; set; }
    }
}
