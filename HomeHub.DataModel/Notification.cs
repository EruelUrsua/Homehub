using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeHub.DataModel
{
    public class Notification
    {
        public int NotificationId { get; set; }  
        public string BusinessId { get; set; }
        public string Message { get; set; } = string.Empty;  // Notification message
        public bool IsRead { get; set; } = false;  // Default is unread (false)
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Timestamp when created
    }
}
