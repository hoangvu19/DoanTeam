using DoAnTeam12.Models.Department;
using DoAnTeam12.Models.Employee;
using DoAnTeam12.Models.Payroll;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace DoAnTeam12.DAL
{
    public class PayrollDAL
    {
        private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
        private string mySqlConn = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private decimal CalculateNetSalary(PayrollModels payroll)
        {
            int totalWorkDaysInMonth = 26;
            decimal baseSalary = payroll.BaseSalary;
            decimal salaryByWorkDays = (baseSalary / totalWorkDaysInMonth) * payroll.WorkDays;
            decimal bonus = 0;
            if (payroll.WorkDays >= 24)
                bonus = baseSalary * 0.10m;
            else if (payroll.WorkDays >= 20)
                bonus = baseSalary * 0.05m;
            decimal deductions = payroll.AbsentDays * (baseSalary * 0.03m);
            decimal netSalary = salaryByWorkDays + bonus - deductions;
            if (netSalary < 0) netSalary = 0;
            payroll.Bonus = bonus;
            payroll.Deductions = deductions;
            return netSalary;
        }

        public List<PayrollModels> GetPayrollsList(string search = null)
        {
            var list = new List<PayrollModels>();
            string sql = @"
        SELECT
            s.SalaryID,
            s.EmployeeID,
            e.FullName,
            d.DepartmentID, 
            d.DepartmentName,
            p.PositionID,
            p.PositionName,
            s.SalaryMonth,
            a.AttendanceID,
            a.AttendanceMonth,
            a.WorkDays,
            a.AbsentDays,
            a.LeaveDays,
            s.BaseSalary,
            s.Bonus,
            s.Deductions,
            s.NetSalary
        FROM salaries s
        JOIN employees e ON s.EmployeeID = e.EmployeeID
        LEFT JOIN attendance a ON s.EmployeeID = a.EmployeeID
            AND MONTH(s.SalaryMonth) = MONTH(a.AttendanceMonth)
            AND YEAR(s.SalaryMonth) = YEAR(a.AttendanceMonth)
        LEFT JOIN departments d ON e.DepartmentID = d.DepartmentID
        LEFT JOIN positions p ON e.PositionID = p.PositionID
        ORDER BY s.SalaryID ASC;";

            using (MySqlConnection conn = new MySqlConnection(mySqlConn))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int workDays = reader.IsDBNull(reader.GetOrdinal("WorkDays")) ? 0 : reader.GetInt32("WorkDays");
                        int absentDays = reader.IsDBNull(reader.GetOrdinal("AbsentDays")) ? 0 : reader.GetInt32("AbsentDays");
                        int leaveDays = reader.IsDBNull(reader.GetOrdinal("LeaveDays")) ? 0 : reader.GetInt32("LeaveDays");

                        workDays = Math.Max(workDays, 0);
                        absentDays = Math.Max(absentDays, 0);
                        leaveDays = Math.Max(leaveDays, 0);

                        int totalDays = workDays + absentDays + leaveDays;
                        if (totalDays > 31)
                        {
                            int over = totalDays - 31;
                            if (absentDays >= over)
                                absentDays -= over;
                            else
                            {
                                over -= absentDays;
                                absentDays = 0;
                                leaveDays = Math.Max(0, leaveDays - over);
                            }
                        }

                        list.Add(new PayrollModels
                        {
                            SalaryID = reader.GetInt32("SalaryID"),
                            EmployeeID = reader.GetInt32("EmployeeID"),
                            FullName = reader.GetString("FullName"),
                            DepartmentID = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? (int?)null : reader.GetInt32("DepartmentID"),
                            DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString("DepartmentName"),
                            PositionID = reader.IsDBNull(reader.GetOrdinal("PositionID")) ? (int?)null : reader.GetInt32("PositionID"),
                            PositionName = reader.IsDBNull(reader.GetOrdinal("PositionName")) ? "" : reader.GetString("PositionName"),
                            SalaryMonth = reader.GetDateTime("SalaryMonth"),
                            AttendanceID = reader.IsDBNull(reader.GetOrdinal("AttendanceID")) ? (int?)null : reader.GetInt32("AttendanceID"),
                            AttendanceMonth = reader.IsDBNull(reader.GetOrdinal("AttendanceMonth")) ? DateTime.MinValue : reader.GetDateTime("AttendanceMonth"),
                            WorkDays = workDays,
                            AbsentDays = absentDays,
                            LeaveDays = leaveDays,
                            BaseSalary = reader.GetDecimal("BaseSalary"),
                            Bonus = reader.GetDecimal("Bonus"),
                            Deductions = reader.GetDecimal("Deductions"),
                            NetSalary = reader.GetDecimal("NetSalary")
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }

            // Lọc theo tên hoặc mã bảng lương nếu có search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                list = list.Where(p =>
                    p.FullName.ToLower().Contains(search) ||
                    p.SalaryID.ToString().Contains(search)
                ).ToList();
            }

            return list.OrderBy(x => x.SalaryID).ToList();
        }





        public List<Department> GetDepartments()
        {
            List<Department> departments = new List<Department>();
            string sql = "SELECT DepartmentID, DepartmentName FROM departments";

            using (var conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    departments.Add(new Department
                    {
                        DepartmentID = reader.GetInt32("DepartmentID"),
                        DepartmentName = reader.GetString("DepartmentName")
                    });
                }
            }

            return departments;
        }

        public List<Position> GetPositions()
        {
            List<Position> positions = new List<Position>();
            string sql = "SELECT PositionID, PositionName FROM positions";

            using (var conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    positions.Add(new Position
                    {
                        PositionID = reader.GetInt32("PositionID"),
                        PositionName = reader.GetString("PositionName")
                    });
                }
            }

            return positions;
        }

        public PayrollModels GetPayrollDetails(int salaryID)
        {
            PayrollModels payroll = null;

            using (MySqlConnection conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();
                string sql = @"
        SELECT
            s.SalaryID,
            s.EmployeeID,
            e.FullName,
            d.DepartmentID,
            d.DepartmentName,
            p.PositionID,
            p.PositionName,
            s.SalaryMonth,
            a.AttendanceID,
            a.AttendanceMonth,
            a.WorkDays,
            a.AbsentDays,
            a.LeaveDays,
            s.BaseSalary,
            s.Bonus,
            s.Deductions,
            s.NetSalary
        FROM salaries s
        JOIN employees e ON s.EmployeeID = e.EmployeeID
        LEFT JOIN attendance a ON s.EmployeeID = a.EmployeeID
            AND MONTH(s.SalaryMonth) = MONTH(a.AttendanceMonth)
            AND YEAR(s.SalaryMonth) = YEAR(a.AttendanceMonth)
        LEFT JOIN departments d ON e.DepartmentID = d.DepartmentID
        LEFT JOIN positions p ON e.PositionID = p.PositionID
        WHERE s.SalaryID = @SalaryID;
        ";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SalaryID", salaryID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payroll = new PayrollModels
                            {
                                SalaryID = reader.GetInt32("SalaryID"),
                                EmployeeID = reader.GetInt32("EmployeeID"),
                                FullName = reader.GetString("FullName"),
                                DepartmentID = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? (int?)null : reader.GetInt32("DepartmentID"),
                                DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? "" : reader.GetString("DepartmentName"),
                                PositionID = reader.IsDBNull(reader.GetOrdinal("PositionID")) ? (int?)null : reader.GetInt32("PositionID"),
                                PositionName = reader.IsDBNull(reader.GetOrdinal("PositionName")) ? "" : reader.GetString("PositionName"),
                                SalaryMonth = reader.GetDateTime("SalaryMonth"),
                                AttendanceID = reader.IsDBNull(reader.GetOrdinal("AttendanceID")) ? (int?)null : reader.GetInt32("AttendanceID"),
                                AttendanceMonth = reader.IsDBNull(reader.GetOrdinal("AttendanceMonth")) ? DateTime.MinValue : reader.GetDateTime("AttendanceMonth"),
                                WorkDays = reader.IsDBNull(reader.GetOrdinal("WorkDays")) ? 0 : reader.GetInt32("WorkDays"),
                                AbsentDays = reader.IsDBNull(reader.GetOrdinal("AbsentDays")) ? 0 : reader.GetInt32("AbsentDays"),
                                LeaveDays = reader.IsDBNull(reader.GetOrdinal("LeaveDays")) ? 0 : reader.GetInt32("LeaveDays"),
                                BaseSalary = reader.GetDecimal("BaseSalary"),
                                Bonus = reader.GetDecimal("Bonus"),
                                Deductions = reader.GetDecimal("Deductions"),
                                NetSalary = reader.GetDecimal("NetSalary")
                            };
                        }
                    }
                }
            }

            return payroll;
        }

        public bool PayrollExists(int employeeId, DateTime salaryMonth)
        {
            using (var conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM salaries WHERE EmployeeID = @EmployeeID AND MONTH(SalaryMonth) = MONTH(@SalaryMonth) AND YEAR(SalaryMonth) = YEAR(@SalaryMonth)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    cmd.Parameters.AddWithValue("@SalaryMonth", salaryMonth);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool EmployeeExists(int employeeId)
        {
            using (var conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM employees WHERE EmployeeID = @EmployeeID";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool AddPayroll(PayrollModels payroll)
        {
            try
            {
                using (var conn = new MySqlConnection(mySqlConn))
                {
                    conn.Open();
                    payroll.NetSalary = CalculateNetSalary(payroll);

                    // 1. Thêm vào bảng salaries, không truyền SalaryID (tự tăng)
                    string sqlSalaries = @"
                INSERT INTO salaries (EmployeeID, SalaryMonth, BaseSalary, Bonus, Deductions, NetSalary)
                VALUES (@EmployeeID, @SalaryMonth, @BaseSalary, @Bonus, @Deductions, @NetSalary);
                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sqlSalaries, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", payroll.EmployeeID);
                        cmd.Parameters.AddWithValue("@SalaryMonth", payroll.SalaryMonth);
                        cmd.Parameters.AddWithValue("@BaseSalary", payroll.BaseSalary);
                        cmd.Parameters.AddWithValue("@Bonus", payroll.Bonus);
                        cmd.Parameters.AddWithValue("@Deductions", payroll.Deductions);
                        cmd.Parameters.AddWithValue("@NetSalary", payroll.NetSalary);

                        // Lấy SalaryID vừa thêm
                        var result = cmd.ExecuteScalar();
                        payroll.SalaryID = Convert.ToInt32(result);
                    }

                    // 2. Thêm vào bảng attendance, không truyền AttendanceID (tự tăng)
                    string sqlAttendance = @"
                INSERT INTO attendance (EmployeeID, AttendanceMonth, WorkDays, AbsentDays, LeaveDays)
                VALUES (@EmployeeID, @AttendanceMonth, @WorkDays, @AbsentDays, @LeaveDays);
                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sqlAttendance, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", payroll.EmployeeID);
                        cmd.Parameters.AddWithValue("@AttendanceMonth", payroll.AttendanceMonth);
                        cmd.Parameters.AddWithValue("@WorkDays", payroll.WorkDays);
                        cmd.Parameters.AddWithValue("@AbsentDays", payroll.AbsentDays);
                        cmd.Parameters.AddWithValue("@LeaveDays", payroll.LeaveDays);

                        // Lấy AttendanceID vừa thêm
                        var result = cmd.ExecuteScalar();
                        payroll.AttendanceID = Convert.ToInt32(result);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to add payroll", ex);
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public string GetEmployeeNameById(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(mySqlConn))
                {
                    conn.Open();
                    string sql = "SELECT FullName FROM employees WHERE EmployeeID = @id";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public bool UpdatePayroll(PayrollModels payroll)
        {
            try
            {
                // Kiểm tra điều kiện trước khi cập nhật
                if (payroll.WorkDays < 0 || payroll.AbsentDays < 0 || payroll.LeaveDays < 0)
                {
                    Console.WriteLine("WorkDays, AbsentDays, LeaveDays không được là số âm.");
                    return false;
                }

                int totalDays = payroll.WorkDays + payroll.AbsentDays + payroll.LeaveDays;
                if (totalDays > 31)
                {
                    Console.WriteLine("Tổng WorkDays + AbsentDays + LeaveDays không được lớn hơn 31.");
                    return false;
                }

                if (payroll.AttendanceMonth > DateTime.Now)
                {
                    Console.WriteLine("Ngày AttendanceMonth không được lớn hơn ngày hiện tại.");
                    return false;
                }

                using (var conn = new MySqlConnection(mySqlConn))
                {
                    conn.Open();

                    // Tính lương thực nhận mới
                    payroll.NetSalary = CalculateNetSalary(payroll);

                    // Cập nhật bảng salaries nếu SalaryID khác 0 (có bản ghi tồn tại)
                    if (payroll.SalaryID != 0)
                    {
                        string sqlUpdateSalary = @"
                    UPDATE salaries SET
                        EmployeeID = @EmployeeID,
                        SalaryMonth = @SalaryMonth,
                        BaseSalary = @BaseSalary,
                        Bonus = @Bonus,
                        Deductions = @Deductions,
                        NetSalary = @NetSalary
                    WHERE SalaryID = @SalaryID";

                        using (var cmd = new MySqlCommand(sqlUpdateSalary, conn))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", payroll.EmployeeID);
                            cmd.Parameters.AddWithValue("@SalaryMonth", payroll.SalaryMonth);
                            cmd.Parameters.AddWithValue("@BaseSalary", payroll.BaseSalary);
                            cmd.Parameters.AddWithValue("@Bonus", payroll.Bonus);
                            cmd.Parameters.AddWithValue("@Deductions", payroll.Deductions);
                            cmd.Parameters.AddWithValue("@NetSalary", payroll.NetSalary);
                            cmd.Parameters.AddWithValue("@SalaryID", payroll.SalaryID);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                return false; // không có bản ghi nào bị ảnh hưởng
                        }
                    }
                    else
                    {
                        // Nếu SalaryID = 0, có thể bạn muốn insert mới hoặc báo lỗi
                        return false;
                    }

                    // Cập nhật bảng attendance nếu AttendanceID khác 0
                    if (payroll.AttendanceID != 0)
                    {
                        string sqlUpdateAttendance = @"
                    UPDATE attendance SET
                        EmployeeID = @EmployeeID,
                        AttendanceMonth = @AttendanceMonth,
                        WorkDays = @WorkDays,
                        AbsentDays = @AbsentDays,
                        LeaveDays = @LeaveDays
                    WHERE AttendanceID = @AttendanceID";

                        using (var cmd = new MySqlCommand(sqlUpdateAttendance, conn))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", payroll.EmployeeID);
                            cmd.Parameters.AddWithValue("@AttendanceMonth", payroll.AttendanceMonth);
                            cmd.Parameters.AddWithValue("@WorkDays", payroll.WorkDays);
                            cmd.Parameters.AddWithValue("@AbsentDays", payroll.AbsentDays);
                            cmd.Parameters.AddWithValue("@LeaveDays", payroll.LeaveDays);
                            cmd.Parameters.AddWithValue("@AttendanceID", payroll.AttendanceID);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                return false; // không có bản ghi nào bị ảnh hưởng
                        }
                    }
                    else
                    {
                        // Nếu AttendanceID = 0, có thể bạn muốn insert mới hoặc báo lỗi
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to update payroll", ex);
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public PayrollModels GetPayrollBySalaryID(int salaryId)
        {
            PayrollModels payroll = null;

            using (MySqlConnection conn = new MySqlConnection(mySqlConn))
            {
                conn.Open();

                string query = @"
                SELECT s.SalaryID, s.EmployeeID, a.AttendanceID, a.WorkDays, a.AbsentDays, a.LeaveDays,
                       a.AttendanceMonth, s.SalaryMonth, s.BaseSalary
                FROM salaries s
                JOIN attendance a ON s.EmployeeID = a.EmployeeID AND MONTH(s.SalaryMonth) = MONTH(a.AttendanceMonth)
                WHERE s.SalaryID = @SalaryID";


                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SalaryID", salaryId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payroll = new PayrollModels
                            {
                                SalaryID = reader.GetInt32("SalaryID"),
                                EmployeeID = reader.GetInt32("EmployeeID"),
                                AttendanceID = reader.GetInt32("AttendanceID"),
                                WorkDays = reader.GetInt32("WorkDays"),
                                AbsentDays = reader.GetInt32("AbsentDays"),
                                LeaveDays = reader.GetInt32("LeaveDays"),
                                AttendanceMonth = reader.GetDateTime("AttendanceMonth"),
                                SalaryMonth = reader.GetDateTime("SalaryMonth"),
                                BaseSalary = reader.GetDecimal("BaseSalary")
                            };
                        }
                    }
                }
            }

            return payroll;
        }

        public bool DeletePayroll(int salaryId)
        {
            try
            {
                using (var conn = new MySqlConnection(mySqlConn))
                {
                    conn.Open();

                    // B1: Lấy EmployeeID và SalaryMonth để biết bản ghi attendance nào cần xóa
                    string selectQuery = "SELECT EmployeeID, SalaryMonth FROM salaries WHERE SalaryID = @salaryId";
                    int empId = 0;
                    DateTime salaryMonth = DateTime.MinValue;

                    using (var cmd = new MySqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@salaryId", salaryId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                empId = reader.GetInt32("EmployeeID");
                                salaryMonth = reader.GetDateTime("SalaryMonth");
                            }
                            else
                            {
                                return false; // Không tìm thấy bản ghi
                            }
                        }
                    }

                    // B2: Xóa attendance tương ứng
                    string deleteAttendance = "DELETE FROM attendance WHERE EmployeeID = @empId AND AttendanceMonth = @month";
                    using (var cmd = new MySqlCommand(deleteAttendance, conn))
                    {
                        cmd.Parameters.AddWithValue("@empId", empId);
                        cmd.Parameters.AddWithValue("@month", salaryMonth);
                        cmd.ExecuteNonQuery(); // Không cần check affectedRows nếu có thể không có dữ liệu
                    }

                    // B3: Xóa bản ghi lương
                    string deleteSalary = "DELETE FROM salaries WHERE SalaryID = @salaryId";
                    using (var cmd = new MySqlCommand(deleteSalary, conn))
                    {
                        cmd.Parameters.AddWithValue("@salaryId", salaryId);
                        int affected = cmd.ExecuteNonQuery();
                        return affected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeletePayroll Error: " + ex.Message);
                return false;
            }
        }




        private void LogError(string message, Exception ex)
        {
            Debug.WriteLine($"{message}: {ex.Message}");
        }
    }
}
