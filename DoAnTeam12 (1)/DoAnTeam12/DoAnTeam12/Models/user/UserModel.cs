using DoAnTeam12.Models.Attendance;
using DoAnTeam12.Models.Employee;
using DoAnTeam12.Models.Payroll;
using System.Collections.Generic;

namespace DoAnTeam12.Models.User
{
    public class UserModel
    {
        public List<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();
        public List<string> AnniversaryAlerts { get; set; } = new List<string>();
        public List<string> SalaryVarianceAlerts { get; set; } = new List<string>();
        public List<string> LeaveAlerts { get; set; } = new List<string>();
        public List<EmployeeModel> Employees { get; set; } = new List<EmployeeModel>();
        public List<PayrollModels> Payrolls { get; set; } = new List<PayrollModels>();
    }
}