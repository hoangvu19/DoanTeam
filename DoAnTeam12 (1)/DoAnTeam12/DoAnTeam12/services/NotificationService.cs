using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using DoAnTeam12.DAL;
using DoAnTeam12.Models.Employee;
using DoAnTeam12.Models.Payroll;
using MailKit.Net.Smtp;
using MimeKit;

namespace DoAnTeam12.Services
{
    public class NotificationService
    {
        private readonly EmployeeDAL _employeeDal;
        private readonly PayrollDAL _payrollDal;
        

        public NotificationService()
        {
            _employeeDal = new EmployeeDAL();
            _payrollDal = new PayrollDAL();

            
        }

        public List<string> GetAnniversaryAlerts(int daysThreshold = 30)
        {
            var alerts = new List<string>();
            var employees = _employeeDal.GetAllEmployees(null, null, null);

            foreach (var emp in employees)
            {
                if (emp.HireDate.HasValue)
                {
                    DateTime anniversary = new DateTime(DateTime.Now.Year, emp.HireDate.Value.Month, emp.HireDate.Value.Day);
                    if (anniversary < DateTime.Now)
                        anniversary = anniversary.AddYears(1);

                    var daysTo = (anniversary - DateTime.Now).TotalDays;
                    if (daysTo <= daysThreshold)
                        alerts.Add($"Employee {emp.FullName} has a work anniversary in {Math.Round(daysTo)} days ({anniversary:dd/MM/yyyy})");
                }
            }

            return alerts;
        }

        public List<string> GetLeaveAlerts(int maxLeaveDays = 15)
        {
            var alerts = new List<string>();
            var payrolls = _payrollDal.GetPayrollsList();

            foreach (var payroll in payrolls)
            {
                if (payroll.LeaveDays > maxLeaveDays)
                    alerts.Add($"Employee {payroll.FullName} has exceeded allowed leave days: {payroll.LeaveDays}");
            }

            return alerts;
        }

        public List<string> GetSalaryVarianceAlerts(decimal threshold = 0.5m)
        {
            var alerts = new List<string>();
            var grouped = _payrollDal.GetPayrollsList().GroupBy(p => p.EmployeeID);

            foreach (var group in grouped)
            {
                var list = group.OrderByDescending(p => p.SalaryMonth).ToList();
                if (list.Count >= 2)
                {
                    decimal curr = list[0].NetSalary;
                    decimal prev = list[1].NetSalary;
                    decimal diff = Math.Abs(curr - prev) / (prev == 0 ? 1 : prev);

                    if (diff > threshold)
                    {
                        alerts.Add($"Employee {list[0].FullName}'s salary changed by {diff:P}: {prev:N0} → {curr:N0} ({list[1].SalaryMonth:MM/yyyy} → {list[0].SalaryMonth:MM/yyyy})");
                    }
                }
            }

            return alerts;
        }

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

        public void SendAlertsToEmployees()
        {
            var employees = _employeeDal.GetAllEmployees(null, null, null);
            var payrolls = _payrollDal.GetPayrollsList();

           
            foreach (var payroll in payrolls)
            {
                if (payroll.LeaveDays > 15)
                {
                    var emp = employees.FirstOrDefault(e => e.EmployeeID == payroll.EmployeeID);
                    if (emp != null && !string.IsNullOrEmpty(emp.Email))
                    {
                        string subject = "Cảnh báo: Số ngày nghỉ vượt quá quy định";
                        string body = $"Xin chào {emp.FullName},<br/><br/>" +
                                      $"Bạn đã nghỉ {payroll.LeaveDays} ngày, vượt quá giới hạn cho phép. Vui lòng chú ý điều chỉnh.<br/><br/>" +
                                      "Trân trọng,<br/>Phòng nhân sự";

                        SendEmail(emp.Email, subject, body);
                    }
                }
            }

           
            var salaryGroups = payrolls.GroupBy(p => p.EmployeeID);
            foreach (var group in salaryGroups)
            {
                var list = group.OrderByDescending(p => p.SalaryMonth).ToList();
                if (list.Count >= 2)
                {
                    decimal curr = list[0].NetSalary;
                    decimal prev = list[1].NetSalary;
                    decimal diff = Math.Abs(curr - prev) / (prev == 0 ? 1 : prev);

                    if (diff > 0.1m)
                    {
                        var emp = employees.FirstOrDefault(e => e.EmployeeID == list[0].EmployeeID);
                        if (emp != null && !string.IsNullOrEmpty(emp.Email))
                        {
                            string subject = "Thông báo: Biến động lương đáng chú ý";
                            string body = $"Xin chào {emp.FullName},<br/><br/>" +
                                          $"Lương tháng này của bạn có sự thay đổi {diff:P} so với tháng trước.<br/>" +
                                          $"Từ {prev:N0} VNĐ ({list[1].SalaryMonth:MM/yyyy}) đến {curr:N0} VNĐ ({list[0].SalaryMonth:MM/yyyy}).<br/><br/>" +
                                          "Trân trọng,<br/>Phòng nhân sự";

                            SendEmail(emp.Email, subject, body);
                        }
                    }
                }
            }

            
            foreach (var emp in employees)
            {
                if (emp.HireDate.HasValue)
                {
                    DateTime anniversary = new DateTime(DateTime.Now.Year, emp.HireDate.Value.Month, emp.HireDate.Value.Day);
                    if (anniversary < DateTime.Now)
                        anniversary = anniversary.AddYears(1);

                    var daysTo = (anniversary - DateTime.Now).TotalDays;
                    if (daysTo <= 30)
                    {
                        if (!string.IsNullOrEmpty(emp.Email))
                        {
                            string subject = "Sắp đến ngày kỷ niệm làm việc";
                            string body = $"Xin chào {emp.FullName},<br/><br/>" +
                                          $"Sắp đến ngày kỷ niệm làm việc của bạn: {anniversary:dd/MM/yyyy} (còn {Math.Round(daysTo)} ngày nữa).<br/><br/>" +
                                          "Trân trọng cảm ơn sự cống hiến của bạn!<br/>Phòng nhân sự";

                            SendEmail(emp.Email, subject, body);
                        }
                    }
                }
            }
        }


    }
}
