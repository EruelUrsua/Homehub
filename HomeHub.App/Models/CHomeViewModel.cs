using HomeHub.DataModel;

namespace HomeHub.App.Models
{
    public class CHomeViewModel
    {
        public List<Provider> ProductProviders { get; set; } = new List<Provider>();
        public List<Provider> ServiceProviders { get; set; } = new List<Provider>();
    }
}
