namespace HomeHub.App.Models
{
    public class RatingItemViewModel
    {
        public int Score { get; set; }
        public string Comments { get; set; }
        public DateTime Date { get; set; }
    }

    public class ProviderRatingsViewModel
    {
        public double AverageRating { get; set; }
        public List<RatingItemViewModel> Ratings { get; set; }
    }
}
