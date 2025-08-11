using DoAnTeam12.DAL;
using DoAnTeam12.Models.Attendance;
using DoAnTeam12.Models.Employee;
using DoAnTeam12.Models.Payroll;
using DoAnTeam12.Models.User;
using DoAnTeam12.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.User
{
    public class UserController : Controller
    {
        private readonly PayrollDAL _payrollDal = new PayrollDAL();
        private readonly EmployeeDAL _employeeDal = new EmployeeDAL();
        private readonly NotificationDAl _notificationDal = new NotificationDAl();
        private readonly NotificationService _notificationService = new NotificationService();

        public ActionResult Index(string keyword, string selectedMonth)
        {
            var model = new UserModel();

            model.Notifications = _notificationDal.GetAllNotifications();
            model.AnniversaryAlerts = _notificationService.GetAnniversaryAlerts();
            model.SalaryVarianceAlerts = _notificationService.GetSalaryVarianceAlerts();
            model.LeaveAlerts = _notificationService.GetLeaveAlerts();

            if (!string.IsNullOrEmpty(keyword))
            {
                model.Employees = _employeeDal.SearchEmployeesFromBothDB(keyword);

                if (model.Employees == null || !model.Employees.Any())
                {
                    ViewBag.ErrorMessage = "No employees found with this ID. Please try again.";
                    model.Payrolls = new List<PayrollModels>();
                }
                else
                {
                    var employeeIds = model.Employees.Select(e => e.EmployeeID).ToList();
                    var allEmployeePayrolls = _payrollDal.GetPayrollsList().Where(p => employeeIds.Contains(p.EmployeeID));

                    DateTime monthToFilter;

                    if (selectedMonth?.ToLower() == "previous" || selectedMonth?.ToLower() == "lastmonth")
                    {
                        monthToFilter = DateTime.Now.AddMonths(-1);
                    }
                    else if (!DateTime.TryParseExact(selectedMonth, "MM/yyyy", null, System.Globalization.DateTimeStyles.None, out monthToFilter))
                    {
                        monthToFilter = DateTime.Now;
                    }

                    allEmployeePayrolls = allEmployeePayrolls.Where(p => p.SalaryMonth.Year == monthToFilter.Year && p.SalaryMonth.Month == monthToFilter.Month);

                    model.Payrolls = allEmployeePayrolls.ToList();
                }
            }
            else
            {
                model.Employees = new List<EmployeeModel>();
                model.Payrolls = new List<PayrollModels>();
            }

            return View(model);
        }
    }
}