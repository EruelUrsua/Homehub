using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class EditUserVM
    {

        public string Id { get; set; }
        public string Lastname { get; set; }
   
        public string Firstname { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }

        [Phone]
        public string ContactNo { get; set; }
    
        public string Address { get; set; }


        public double lat { get; set; }


        public double lng { get; set; }

        public string? Role { get; set; }
        public string? ValidId { get; set; }
        //public string? BusinessPermit { get; set; }

    }
}

