using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.DataModel;

public partial class Service
{
    public string ServiceId { get; set; } = null!;

    public string ServiceItem { get; set; } = null!;

    public string Details { get; set; } = null!;

    public decimal Fee { get; set; }

    public bool Available { get; set; }

    public int ProviderID { get; set; }
}
