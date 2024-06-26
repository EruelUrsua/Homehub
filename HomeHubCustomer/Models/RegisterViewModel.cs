using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterViewModel
    {
        public RegisterViewModel()
        {
            Firstname = "";
            Email = "";
            Password = "";
            Usertype = "Customer";
        }

        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [Required]
        public string ValidIDno { get; set; }


        [Required]
        public int ContactNo { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /*[Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; */
        [Required]
        public string Usertype { get; set; }
    }
}
