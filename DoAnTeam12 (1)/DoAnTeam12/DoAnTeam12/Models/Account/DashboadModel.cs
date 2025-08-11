using DoAnTeam12.Models.Department;
using DoAnTeam12.Models.Employee;
using DoAnTeam12.Models.Payroll;
using DoAnTeam12.Models.Account;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DoAnTeam12.Models.Account
{
    public class DepartmentEmployeeCount
    {
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class MonthlyAveragePayroll
    {
        public string MonthLabel { get; set; }
        public decimal AverageSalary { get; set; }
    }


    public class DashboardModel
    {
        public List<PayrollModels> Payrolls { get; set; }

        public List<EmployeeModel> Employees { get; set; }

        public List<DepartmentModel> Department { get; set; }

        public List<UserModel> Auth { get; set; }

        public List<DepartmentEmployeeCount> DepartmentEmployeeCounts { get; set; }

        public List<MonthlyAveragePayroll> MonthlyAveragePayrolls { get; set; }


        public int TotalPayrollRecords => Payrolls?.Count ?? 0;

        public int TotalEmployees => Employees?.Count ?? 0;

        public int TotalAccounts => Auth?.Count ?? 0;

        public int TotalDepartments => Department?.Count ?? 0;

        public decimal TotalNetSalary
        {
            get
            {
                return Payrolls?.Sum(p => p.NetSalary) ?? 0m;
            }
        }
    }
}