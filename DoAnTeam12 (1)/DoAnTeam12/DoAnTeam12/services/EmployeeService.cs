using DoAnTeam12.DAL;
using DoAnTeam12.Models.Employee;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

public class EmployeeService
{
    private EmployeeDAL employeeDAL = new EmployeeDAL();
    private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
    private void SendEmail(string to, string subject, string body)
    {
        try
        {
            var email = new DoAnTeam12.Models.SendEmail.Gmail
            {
                To = to,
                Subject = subject,
                Body = body,
            };
            email.SendEmail();
            Debug.WriteLine($"Email sent successfully to {to} with subject: {subject}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending email to {to}: {ex.Message}");
        }
    }

    public bool AddAndWelcomeEmployee(EmployeeModel employee)
    {
        using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
        {
            sqlConn.Open();
            employeeDAL.INEmailPhone(employee);

            try
            {
                employeeDAL.InsertNhanVien(employee);

                string subject = "Chào mừng bạn đến với Công Ty Everest";
                string body = $"<p>Chào <strong>{employee.FullName}</strong>,</p>" +
                              "<p>Bạn đã được thêm vào hệ thống nhân sự của chúng tôi.</p>" +
                              "<p>Vui lòng đăng nhập để cập nhật thông tin cá nhân và xem bảng lương.</p>";

                SendEmail(employee.Email, subject, body);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding employee {employee.FullName}: {ex.Message}");
                return false;
            }
        }
    }
}
