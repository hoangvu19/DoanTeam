using DoAnTeam12.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnTeam12.Models.Attendance
{
    public class NotificationModel
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string BadgeText { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DaysUntilBirthday { get; set; }
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string Type { get; set; }
    }
}