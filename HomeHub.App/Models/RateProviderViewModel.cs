﻿using Azure.Core.Pipeline;
using HomeHub.DataModel;

namespace HomeHub.App.Models
{
    public class RateProviderViewModel
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int Score { get; set; }
        public string Comments { get; set; }
        public DateOnly Date { get; set; }
        public string BusinessId { get; set; }
    }
}