using HomeHub.DataModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HomeHub.App.Models
{
    public class NotificationVM
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(50)]
        public string BusinessId { get; set; }  

        [Required]
        [StringLength(255)]
        public string Message { get; set; }  // Notification message

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // When the notification was created

        public bool IsRead { get; set; } = false;  // Has the provider seen this notification?
    }
}
