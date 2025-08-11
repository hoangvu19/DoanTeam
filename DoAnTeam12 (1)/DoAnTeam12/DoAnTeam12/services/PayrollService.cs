using DoAnTeam12.DAL;
using DoAnTeam12.Models.Payroll;
using DoAnTeam12.Models.Employee;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using System;

namespace DoAnTeam12.Services
{
    public class PayrollService
    {
        private PayrollDAL payrollDAL = new PayrollDAL();
        private EmployeeDAL employeeDAL = new EmployeeDAL();

        private void SendEmail(string to, string subject, string body)
        {
            var email = new DoAnTeam12.Models.SendEmail.Gmail
            {
                To = to,
                Subject = subject,
                Body = body
            };

            email.SendEmail();
        }

        public bool AddPayrollAndNotify(PayrollModels payroll)
        {
            bool result = payrollDAL.AddPayroll(payroll);
            if (result)
            {
                string formattedSalaryMonth = payroll.SalaryMonth.ToString("MM/yyyy");
                string employeeEmail = employeeDAL.GetEmployeeEmailById(payroll.EmployeeID);
                string payrollname =payrollDAL.GetEmployeeNameById(payroll.EmployeeID);

                if (!string.IsNullOrEmpty(employeeEmail))
                {
                    string subject = $"[Công Ty Everest ] Bảng lương tháng {formattedSalaryMonth}";
                    string body = $"Chào {payrollname},\n\n" +
                                  $"Lương tháng {payroll.SalaryMonth} của bạn đã được thêm.\n" +
                                  $"- Lương cơ bản: {payroll.BaseSalary:N0} VND\n" +
                                  $"- Thưởng: {payroll.Bonus:N0} VND\n" +
                                  $"- Khấu trừ: {payroll.Deductions:N0} VND\n" +
                                  $"- Tổng lương: {payroll.NetSalary:N0} VND\n\n" +
                                  $"Vui lòng kiểm tra chi tiết trong hệ thống.";

                    SendEmail(employeeEmail, subject, body);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Email not found for EmployeeID {payroll.EmployeeID}");
                }
            }

            return result;
        }
        public bool Update(PayrollModels payroll)
        {
            bool result = payrollDAL.UpdatePayroll(payroll);

            if (result)
            {
                string formattedSalaryMonth = payroll.SalaryMonth.ToString("MM/yyyy");
                string employeeEmail = employeeDAL.GetEmployeeEmailById(payroll.EmployeeID);
                string payrollname = payrollDAL.GetEmployeeNameById(payroll.EmployeeID);

                if (!string.IsNullOrEmpty(employeeEmail))
                {
                    string subject = $"[Công Ty Everest ] Bảng lương tháng {formattedSalaryMonth}";
                    string body = $"Chào {payrollname},<br><br>" +
                                  $"Lương tháng **{formattedSalaryMonth}** của bạn đã được cập nhập .<br><br>" + 
                                  $"- Lương cơ bản: {payroll.BaseSalary:N0} VND <br><br>" +
                                  $"- Thưởng: {payroll.Bonus:N0} VND <br><br>" +
                                  $"- Khấu trừ: {payroll.Deductions:N0} VND <br><br>" +
                                  $"- Tổng lương: {payroll.NetSalary:N0} VND <br><br>" +
                                  $"Vui lòng kiểm tra chi tiết trong hệ thống.";

                    SendEmail(employeeEmail, subject, body);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Email not found for EmployeeID {payroll.EmployeeID}");
                }
            }
            return result;
        }
    }
}
