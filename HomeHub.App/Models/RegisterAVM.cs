using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterAVM
    {
        public RegisterAVM()
        {
            Lastname = "";
            Firstname = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";
            ContactNo = "";
            Address = "";
            lat = 14.58699;
            lng = 120.98634;
        }

        [Required]
        [MaxLength]
        public string Lastname { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string ContactNo { get; set; }
        [Required]
        public string Address { get; set; }

        [Required]
        public double lat { get; set; }

        [Required]
        public double lng { get; set; }


        [Required(ErrorMessage = "Password is required only during registration")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}

