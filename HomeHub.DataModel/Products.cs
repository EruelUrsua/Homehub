using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.DataModel;

public class Product
{
    public string ProductId { get; set; }

    public string ProductItem { get; set; } = null!;

    public int Qty { get; set; }
    public decimal Price { get; set; }

    public string ContainerType { get; set; } = null!;

    public int ProviderID { get; set; }
}
