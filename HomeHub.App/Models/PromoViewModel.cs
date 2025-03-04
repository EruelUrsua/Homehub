using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class PromoViewModel
    {
        public int PromoID { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Promo name cannot exceed 10 characters.")]
        public string PromoName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Promo code cannot exceed 10 characters.")]
        public string PromoCode { get; set; }

        [Required(ErrorMessage = "Promo Start Date is required.")]
        [DataType(DataType.Date)]
        public DateTime PromoStart { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Promo End Date is required.")]
        [DataType(DataType.Date)]
        public DateTime PromoEnd { get; set; } = DateTime.Today.AddDays(1);

        public string? BusinessName { get; set; }

        [Required(ErrorMessage = "Discount is required.")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0% and 100%.")]
        public decimal Discount { get; set; }
        public string? BusinessId { get; set; }
    }
}
