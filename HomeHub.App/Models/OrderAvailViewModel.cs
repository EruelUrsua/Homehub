using HomeHub.DataModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class OrderAvailViewModel
    {
        public int businessId { get; set; }
        public string chosen { get; set; }
        public string price { get; set; }
        [Required(ErrorMessage = "Please select a date.")]
        public string ddeliv { get; set; }
        [Required(ErrorMessage = "Please select a time.")]
        public string tdeliv { get; set; }
        [StringLength(150, ErrorMessage = "Cannot exceed 150 characters.")]
        public string? requestatt { get; set; }
        public string mode { get; set; }
        public string? promo { get; set; }
        public int qty { get; set; }
        public decimal discount { get; set; }
        public decimal totalPrice { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        //Logged in user's id
        public string userID { get; set; }
    }
}