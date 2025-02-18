using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class PromoViewModel
    {
        public int PromoID { get; set; }

        [Required(ErrorMessage = "Promo Name is required.")]
        public string PromoName { get; set; }

        [Required(ErrorMessage = "Promo Code is required.")]
        public string PromoCode { get; set; }

        [Required(ErrorMessage = "Promo Start Date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime PromoStart { get; set; }

        [Required(ErrorMessage = "Promo End Date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime PromoEnd { get; set; }

        [Required(ErrorMessage = "Business Name is required.")]
        public string BusinessName { get; set; }

        [Required(ErrorMessage = "Discount is required.")]
        public decimal Discount { get; set; }
    }
}
