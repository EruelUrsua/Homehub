using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;

namespace HomeHub.DataModel
{
    public class RefundRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("RefundID")]
        public int RefundId { get; set; }

        [Required]
        [Column("OrderID")]
        [MaxLength(50)]
        public string OrderId { get; set; }

        // 🔹 Store ASP.NET Identity UserId 
        public string UserId { get; set; }

        [Required]
        [Column("BusinessID")]
        public int BusinessId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Item { get; set; }

        [Required]
        public int RefundQuantity { get; set; }

        [Required]
        public string RefundReason { get; set; }

        [MaxLength(50)]
        public string? RefundStatus { get; set; }

        [Required]
        public DateTime RefundRequestDate { get; set; }

        public DateTime? RefundActionDate { get; set; }

        [Required]
        [Column(TypeName = "MONEY")]
        public decimal Fee { get; set; }

        [MaxLength(10)]
        public string? PromoCode { get; set; } 

        [Column(TypeName = "MONEY")]
        public decimal? RefundAmount { get; set; }
        public string? RejectionReason { get; set; }
    }
}
