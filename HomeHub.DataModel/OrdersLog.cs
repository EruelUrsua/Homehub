using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeHub.DataModel;

public class OrdersLog
{
    public string LogId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public DateTime OrderDate { get; set; }
    // 🔹 Store ASP.NET Identity UserId 
    public string UserId { get; set; } = null!;

    public string FirstName { get; set; } = null!; 

    public string LastName { get; set; } = null!; 

    public string BusinessId { get; set; } 

    public string Item { get; set; } = null!; 

    public int Qty { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = null!; 

    public decimal Fee { get; set; }

    public string PromoCode { get; set; } = null!;


    [NotMapped]
    public string ProviderName { get; set; } = string.Empty;
    [NotMapped]
    public bool IsRated { get; set; }


    public double? lat { get; set; }

    public double? lng { get; set; }
}
