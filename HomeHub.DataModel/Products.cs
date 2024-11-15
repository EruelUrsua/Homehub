using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.DataModel;

public partial class Product
{
    [Required(ErrorMessage = "Product ID is required.")]
    [StringLength(50, ErrorMessage = "Product ID cannot exceed 50 characters.")]
    public string ProductId { get; set; } = null!;

    [Required(ErrorMessage = "Product Item is required.")]
    [StringLength(100, ErrorMessage = "Product Item cannot exceed 100 characters.")]
    public string ProductItem { get; set; } = null!;

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Qty { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Container Type is required.")]
    [StringLength(50, ErrorMessage = "Container Type cannot exceed 50 characters.")]
    public string ContainerType { get; set; } = null!;

    [Required(ErrorMessage = "Provider ID is required.")]
    public int ProviderID { get; set; }
}
