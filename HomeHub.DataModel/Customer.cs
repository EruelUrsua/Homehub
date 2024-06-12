using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Customer
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string ContactNo { get; set; } = null!;

    public string Address { get; set; } = null!;
}
