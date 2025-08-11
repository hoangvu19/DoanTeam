using DoAnTeam12.DAL;
using DoAnTeam12.Models.Account;
using DoAnTeam12.Models.Payroll;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Web.Helpers;


namespace DoAnTeam12.Controllers.Dashboard
{
    public class DashboardController : Controller
    {
        private PayrollDAL payrollDal = new PayrollDAL();
        private EmployeeDAL employeeDal = new EmployeeDAL();
        private AuthDAL authDal = new AuthDAL();
        private DepartmentDAL departmentDal = new DepartmentDAL();

        public ActionResult Dashboard()
        {
            var payrolls = payrollDal.GetPayrollsList();
            var employees = employeeDal.GetAllEmployees(null, null, null);
            var accountsList = authDal.GetTotal();
            var departments = departmentDal.GetAllDepartments();

            var departmentalCounts = employees
                .Where(e => !string.IsNullOrEmpty(e.DepartmentName))
                .GroupBy(e => e.DepartmentName)
                .Select(g => new DepartmentEmployeeCount
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count()
                })
                .OrderBy(d => d.DepartmentName)
                .ToList();

            var twelveMonthsAgo = DateTime.Today.AddMonths(-11).Date;

            var monthlyAveragePayrolls = payrolls
                .Where(p => p.SalaryMonth.Date >= twelveMonthsAgo)
                .GroupBy(p => new { p.SalaryMonth.Year, p.SalaryMonth.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyAveragePayroll
                {
                    MonthLabel = $"{g.Key.Month:D2}/{g.Key.Year}",
                    AverageSalary = g.Average(p => p.NetSalary)
                })
                .ToList();

            var model = new DashboardModel
            {
                Payrolls = payrolls,
                Employees = employees,
                Department = departments,
                Auth = accountsList,
                DepartmentEmployeeCounts = departmentalCounts,
                MonthlyAveragePayrolls = monthlyAveragePayrolls
            };

            return View(model);
        }
    }
}