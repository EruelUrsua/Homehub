using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ServiceVM
    {
        public string ServiceId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Service name can't be longer than 50 characters.")]
        public string ServiceItem { get; set; }

        [StringLength(100, ErrorMessage = "Details can't be longer than 100 characters.")]
        public string Details { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Fee must be a positive number.")]
        public decimal Fee { get; set; }

        public bool Available { get; set; }

        public int ProviderID { get; set; }  // Link to business provider
    }
}
