using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.DataModel;

public partial class Service
{
    [Required(ErrorMessage = "Service ID is required.")]
    [StringLength(50, ErrorMessage = "Service ID cannot exceed 50 characters.")]
    public string ServiceId { get; set; } = null!;

    [Required(ErrorMessage = "Service Item is required.")]
    [StringLength(100, ErrorMessage = "Service Item cannot exceed 100 characters.")]
    public string ServiceItem { get; set; } = null!;

    [Required(ErrorMessage = "Details are required.")]
    [StringLength(500, ErrorMessage = "Details cannot exceed 500 characters.")]
    public string Details { get; set; } = null!;

    [Required(ErrorMessage = "Fee is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fee must be a positive number.")]
    public decimal Fee { get; set; }

    public bool Available { get; set; }

    [Required(ErrorMessage = "Provider ID is required.")]
    public int ProviderID { get; set; }
}
