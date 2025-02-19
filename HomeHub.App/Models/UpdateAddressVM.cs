using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class UpdateAddressVM
    {
        [Required]
        public string Address { get; set; }
    }
}
