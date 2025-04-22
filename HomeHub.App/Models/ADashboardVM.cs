namespace HomeHub.App.Models
{
    public class ADashboardVM
    {
       public int customers { get; set; }

        public int providers { get; set; }

        //public int transactions { get; set; }

        public int userRestricted { get; set; }

        public int userVerified { get; set; }

        public int userReview { get; set; }

        public int totalUser { get; set; }

        //public DateTime orderDates { get; set; }
        public int orderLogs { get; set; }
    }
}
