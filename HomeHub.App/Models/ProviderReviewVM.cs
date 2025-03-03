using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class ProviderReviewVM
    {
        public string OrderId { get; set; }

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Score must be between 1 and 5.")]
        public int Score { get; set; }

        [Required]
        public string Comments { get; set; }
    }
}
