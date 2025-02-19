﻿using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public class ClientOrder
{
    public int ClientId { get; set; }

    public string BusinessId { get; set; } 

    public DateTime OrderDate { get; set; }

    public DateTime Schedule { get; set; }

    public string OrderedPs { get; set; } 

    public decimal Fee { get; set; }

    public string Status { get; set; }

    public string PromoCode { get; set; } 

    public string UserId { get; set; }

    public int RatingId { get; set; }

    public int ReportId { get; set; } 

    public int Quantity { get; set; }
    public string ModeOfPayment { get; set; } 

    public string FirstName { get; set; } 
    public string LastName { get; set; } 


}
