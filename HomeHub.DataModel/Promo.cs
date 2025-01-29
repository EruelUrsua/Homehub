using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeHub.DataModel
{
    public class Promo
    {
        public int PromoId { get; set; }

        public string PromoName { get; set; } = null!;

        public string PromoCode { get; set; } = null!;

        public DateTime PromoStart { get; set; }

        public DateTime PromoEnd { get; set; }

        public string BusinessName { get; set; } = null!;
        public decimal Discount { get; set; }
    }
}
