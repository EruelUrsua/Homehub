using System;
using System.Collections.Generic;

namespace HomeHub.DataModel;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string ProductItem { get; set; } = null!;

    public int Qty { get; set; }

    public decimal Price { get; set; }

    public string ContainerType { get; set; } = null!;
}
