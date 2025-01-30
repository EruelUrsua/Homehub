using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterCVM
    {
        public RegisterCVM()
        {
            Lastname = "";
            Firstname = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";
            ContactNo = 0;
        }

        [Required]
        [MaxLength]
        public string Lastname { get; set; }
        public string Firstname { get; set; }

        public string Email { get; set; }   

        public int ContactNo { get; set; }  

        public string Password { get; set; }

        public string ConfirmPassword { get; set; } 
    }
}
