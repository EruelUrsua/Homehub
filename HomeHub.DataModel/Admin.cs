﻿using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Admin
{
    public string AdminId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
