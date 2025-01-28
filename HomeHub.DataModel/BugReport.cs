using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public class BugReport
{
    public string BugId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string FunctionId { get; set; } = null!;
}
