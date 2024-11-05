using HomeHub.DataModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace HomeHub.App.Models
{
    public class OrderAvailViewModel
    {
        public int businessId { get; set; }
        public string chosen { get; set; }
        public string price { get; set; }
        public string ddeliv { get; set; }
        public string tdeliv { get; set; }
        public string requestatt { get; set; }
        public string mode { get; set; }
        public string? promo { get; set; }
        public int qty { get; set; }
        public decimal discount { get; set; }
        public decimal totalPrice { get; set; }

        //Logged in user's id
        public string userID { get; set; }
    }
}
