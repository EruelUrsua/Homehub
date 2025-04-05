using HomeHub.DataModel;

namespace HomeHub.App.Models
{
    public class UserRatingViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Rating> Ratings { get; set; }
        public double AverageRating { get; set; }
    }
}
