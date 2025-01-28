using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.DataModel;

public class Customer
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string ContactNo { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ValidIDno { get; set; }

    //public ApplicationUser ApplicationUser { get; set; }
}
