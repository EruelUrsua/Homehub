using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class ClientOrder
{
    public string ClientId { get; set; } = null!;

    public string BusinessId { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime Schedule { get; set; }

    public string OrderedPs { get; set; } = null!;

    public decimal Fee { get; set; }

    public bool Status { get; set; }

    public string PromoCode { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RatingId { get; set; } = null!;

    public string ReportId { get; set; } = null!;

    public int Quantity { get; set; }
}
