using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public class Rating
{
    public int RatingId { get; set; }

    public string OrderId { get; set; }
    public string BusinessId { get; set; }

    public int Score { get; set; }

    public string Comments { get; set; } = null!;

    public DateTime Date { get; set; }
}
