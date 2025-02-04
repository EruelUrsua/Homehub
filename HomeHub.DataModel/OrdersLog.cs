using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeHub.DataModel;

public class OrdersLog
{
    public string LogId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string BusinessId { get; set; } = null!;

    public string Item { get; set; } = null!;

    public int Qty { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!;

    public decimal Fee { get; set; }

    public string PromoCode { get; set; } = null!;


    // Temporary property to track if the order has been rated
    [NotMapped]
    public bool IsRated { get; set; }
}
