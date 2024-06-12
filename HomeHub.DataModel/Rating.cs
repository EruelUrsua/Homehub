using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Rating
{
    public string RatingId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int Score { get; set; }

    public string Comments { get; set; } = null!;

    public DateTime Date { get; set; }
}
