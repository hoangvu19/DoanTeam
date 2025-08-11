using DoAnTeam12.Models.Attendance;
using DoAnTeam12.Models.Employee;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace DoAnTeam12.DAL
{
    public class NotificationDAl
    {
        private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;

        public List<NotificationModel> GetAllNotifications()
        {
            var notifications = new List<NotificationModel>();

            if (string.IsNullOrEmpty(sqlServerConn))
            {
                return notifications;
            }

            using (SqlConnection connection = new SqlConnection(sqlServerConn))
            {
                try
                {
                    connection.Open();

                    string announcementQuery = "SELECT AnnouncementID, Title, Link, Description, BadgeText , UpdatedDate FROM dbo.Announcements ORDER BY UpdatedDate DESC";
                    using (SqlCommand announcementCommand = new SqlCommand(announcementQuery, connection))
                    {
                        using (SqlDataReader announcementReader = announcementCommand.ExecuteReader())
                        {
                            while (announcementReader.Read())
                            {
                                notifications.Add(new NotificationModel
                                {
                                    AnnouncementID = Convert.ToInt32(announcementReader["AnnouncementID"]),
                                    Title = announcementReader["Title"].ToString(),
                                    Link = announcementReader["Link"]?.ToString(),
                                    Description = announcementReader["Description"]?.ToString(),
                                    BadgeText = announcementReader["BadgeText"]?.ToString(),
                                    UpdatedDate = announcementReader["UpdatedDate"] as DateTime?,
                                    Type = "Announcement"
                                });
                            }
                        }
                    }

                    string birthdayQuery = @"SELECT EmployeeID, FullName, DateOfBirth
                                             FROM HUMAN.dbo.Employees
                                             WHERE DateOfBirth IS NOT NULL
                                               AND DATEADD(year, DATEDIFF(year, DateOfBirth, GETDATE()), DateOfBirth) >= CAST(GETDATE() AS DATE)
                                               AND DATEADD(year, DATEDIFF(year, DateOfBirth, GETDATE()), DateOfBirth) < DATEADD(day, 30, CAST(GETDATE() AS DATE))";
                    using (SqlCommand birthdayCommand = new SqlCommand(birthdayQuery, connection))
                    {
                        using (SqlDataReader birthdayReader = birthdayCommand.ExecuteReader())
                        {
                            while (birthdayReader.Read())
                            {
                                DateTime? dateOfBirth = birthdayReader["DateOfBirth"] == DBNull.Value
                                    ? (DateTime?)null
                                    : Convert.ToDateTime(birthdayReader["DateOfBirth"]);

                                int? daysUntilBirthday = null;
                                if (dateOfBirth.HasValue)
                                {
                                    DateTime today = DateTime.Today;
                                    DateTime birthdayThisYear = new DateTime(today.Year, dateOfBirth.Value.Month, dateOfBirth.Value.Day);

                                    if (birthdayThisYear < today)
                                    {
                                        daysUntilBirthday = (int)(birthdayThisYear.AddYears(1) - today).TotalDays;
                                    }
                                    else
                                    {
                                        daysUntilBirthday = (int)(birthdayThisYear - today).TotalDays;
                                    }
                                }

                                notifications.Add(new NotificationModel
                                {
                                    EmployeeID = Convert.ToInt32(birthdayReader["EmployeeID"]),
                                    FullName = birthdayReader["FullName"].ToString(),
                                    DateOfBirth = dateOfBirth,
                                    DaysUntilBirthday = daysUntilBirthday,
                                    Type = "Birthday"
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when querying notifications (DAL): {ex.Message}");
                    return new List<NotificationModel>();
                }
            }

            notifications = notifications
                .OrderBy(n => n.Type == "Birthday" ? 0 : 1)
                .ThenBy(n => n.Type == "Birthday" ? n.DaysUntilBirthday : int.MaxValue)
                .ThenByDescending(n => n.UpdatedDate)
                .ToList();

            return notifications;
        }
    }
}