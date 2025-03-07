﻿using HomeHub.DataModel;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class InStorePurchaseVM
    {
        public List<ProductSelectionVM> Products { get; set; } = new List<ProductSelectionVM>();
        public List<ProductSelectionVM> SelectedProducts { get; set; } = new List<ProductSelectionVM>();
    }

    public class ProductSelectionVM
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
