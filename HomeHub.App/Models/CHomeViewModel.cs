using HomeHub.DataModel;

namespace HomeHub.App.Models
{
    public class CHomeViewModel
    {
        public List<Business> ProductProviders { get; set; } = new List<Business>();
        public List<Business> ServiceProviders { get; set; } = new List<Business>();
    }
}
