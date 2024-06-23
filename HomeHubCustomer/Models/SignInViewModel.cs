using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class SignInViewModel
    {
        public SignInViewModel()
        {
            Email = "";
            Password = "";
            ReturnUrl = "";

        }

        [Required]

        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }


    }
}
