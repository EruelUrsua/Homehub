using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class SignInViewModel
    {
        public SignInViewModel()
        {
            Username = "";
            Password = "";
            ReturnUrl = "";
        }

        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
