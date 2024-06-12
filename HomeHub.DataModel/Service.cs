using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Service
{
    public string ServiceId { get; set; } = null!;

    public string ServiceItem { get; set; } = null!;

    public string Details { get; set; } = null!;

    public decimal Fee { get; set; }

    public bool Available { get; set; }
}
