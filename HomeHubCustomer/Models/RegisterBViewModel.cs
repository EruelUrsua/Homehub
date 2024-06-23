using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterBViewModel
    {
        public RegisterBViewModel()
        {
            Email = "";
            CompanyName = "";
            Address = "";
            BusinessPermitNo = "";
            Businesstype = "";
            Password = "";
            Usertype = "Business";
        }


        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required]
        public string CompanyName { get; set; }


        [Required]
        public string Address { get; set; }


        [Required]
        public string BusinessPermitNo { get; set; }

        [Required]
        public string Businesstype { get; set; }


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

