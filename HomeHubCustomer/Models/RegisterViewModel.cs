using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterViewModel
    {
        public RegisterViewModel()
        {
            Email = "";
            Password = "";
            ConfirmPassword = "";
            Usertype = "";
        }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Usertype { get; set; }
    }
}
