using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class UserProfileVM
    {

        public UserProfileVM()
        {
            Roles = new List<string>();
        }

        public string? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //[Required]
        //public string Address { get; set; }

        public double lat { get; set; }

        public double lng { get; set; }

        public IList<string>? Roles { get; set; }

        public string? Role { get; set; }
        public string? ValidId { get; set; }
        public string? BusinessPermit {  get; set; } 

    }
}
