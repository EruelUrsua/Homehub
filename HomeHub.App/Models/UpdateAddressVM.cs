using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class UpdateAddressVM
    {
        [Required]
        public double lat { get; set; }

        [Required]
        public double lng { get; set; }
    }
}
