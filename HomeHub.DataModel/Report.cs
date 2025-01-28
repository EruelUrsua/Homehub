using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public class Report
{
    public string ReportId { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string UserId { get; set; } = null!;
}
