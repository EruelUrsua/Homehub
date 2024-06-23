using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Business
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string BusinessName { get; set; } = null!;

    public string RepresentativeName { get; set; } = null!;

    public int ContactNo { get; set; }

    public string OfferList { get; set; } = null!;
    public char BusinessType { get; set; }

    public ApplicationUser ApplicationUser { get; set; }
}
