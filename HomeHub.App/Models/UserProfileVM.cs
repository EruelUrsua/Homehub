using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class UserProfileVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
