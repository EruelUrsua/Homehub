namespace HomeHub.App.Models
{
    public class ReviewRatingVM
    {
        public int OrderId { get; set; }
        public int Score { get; set; }
        public string Comments { get; set; }
        public string BusinessName { get; set; }
    }
}
