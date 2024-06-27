using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class ClientOrder
{
    public int ClientId { get; set; }

    public string BusinessId { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime Schedule { get; set; }

    public string OrderedPs { get; set; } = null!;

    public decimal Fee { get; set; }

    public bool Status { get; set; }

    public string PromoCode { get; set; } = null!;

    public int UserId { get; set; }

    public int RatingId { get; set; }

    public int ReportId { get; set; } 

    public int Quantity { get; set; }
    public string ModeOfPayment { get; set; } = null!;


}
