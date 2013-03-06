using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inBloom.Models.Home
{
    public class ScheduleParentTeacherConferenceModel
    {
        public string StudentId { get; set; }
        public string ParentId { get; set; }
        public DateTime DateTime { get; set; }
        public string Subject { get; set; }
    }
}