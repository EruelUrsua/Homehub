using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class RegisterBViewModel
    {
        //public RegisterBViewModel()
        //{
        //    Email = "";
        //    CompanyName = "";
        //    Address = "";
        //    BusinessPermitNo = "";
        //    Businesstype = "";
        //    Password = "";
        //    Usertype = "Business";
        //}
        [Required]
        public int UserID { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;


        [Required]
        public string BusinessName { get; set; } = null!;


        [Required]
        public string BusinessPermitNo { get; set; } = null!;

      [Required]
        public char Businesstype { get; set; }
        [Required]

        public string RepresentativeName { get; set; } = null!;
        [Required]
        public string OfferList { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required]
        public string CompanyAddress { get; set; } = null!;
        [Required]

        public String ContactNo { get; set; } = null!;



        /*[Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; */
        //[Required]
        //public string Usertype { get; set; }

    }
}




