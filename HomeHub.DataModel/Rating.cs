using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Rating
{
    public int RatingId { get; set; }

    public int OrderId { get; set; }
    public int BusinessId { get; set; }

    public int Score { get; set; }

    public string Comments { get; set; } = null!;

    public DateTime Date { get; set; }
}
