using DoAnTeam12.Models.Employee;
using System;
using System.Collections.Generic;

namespace DoAnTeam12.Models.Payroll
{
    public class PayrollModels
    {
        public List<EmployeeModel> Employees { get; set; } = new List<EmployeeModel>();
        public int SalaryID { get; set; }
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public int? DepartmentID { get; set; }
        public int? PositionID { get; set; }
        public DateTime SalaryMonth { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public int? AttendanceID { get; set; }
        public int WorkDays { get; set; }
        public int LeaveDays { get; set; }
        public int AbsentDays { get; set; }
        public DateTime AttendanceMonth { get; set; }
        public string Email { get; set; }

    }

    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
    }

    public class Position
    {
        public int PositionID { get; set; }
        public string PositionName { get; set; }
    }

}

