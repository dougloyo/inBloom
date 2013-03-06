using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inBloom.Models.Home
{
    [Serializable]
    public class NotificationModel
    {
        public string StudentId { get; set; }
        public string Subject { get; set; }
        public bool FeedbackPositive { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class NotificationViewModel : NotificationModel
    {
        public string StudentId { get; set; }
        public List<NotificationModel> Notifications { get; set; } 
    }
}