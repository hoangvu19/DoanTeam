using DoAnTeam12.Models.Department;
using DoAnTeam12.Models.Employee;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DoAnTeam12.DAL
{
    public class EmployeeDAL
    {
        private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;
        private string mySqlConn = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public List<EmployeeModel> GetAllEmployees(int? departmentId, int? positionId, string searchString = null)
        {
            List<EmployeeModel> employees = new List<EmployeeModel>();
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                try
                {
                    sqlConn.Open();
                    string query = @"
                        SELECT e.EmployeeID, e.FullName, e.DateOfBirth, e.Gender, e.PhoneNumber, e.Email, e.HireDate, e.DepartmentID, e.PositionID, e.Status, e.CreatedAt, e.UpdatedAt,
                                 d.DepartmentName, p.PositionName
                        FROM Employees e
                        LEFT JOIN Departments d ON e.DepartmentID = d.DepartmentID
                        LEFT JOIN Positions p ON e.PositionID = p.PositionID
                        WHERE (@DepartmentID IS NULL OR e.DepartmentID = @DepartmentID)
                          AND (@PositionID IS NULL OR e.PositionID = @PositionID)
                          AND (@SearchString IS NULL OR e.FullName LIKE @SearchString OR CAST(e.EmployeeID AS NVARCHAR) LIKE @SearchString)";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", (object)departmentId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PositionID", (object)positionId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SearchString", string.IsNullOrEmpty(searchString) ? (object)DBNull.Value : "%" + searchString + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new EmployeeModel()
                                {
                                    EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                    FullName = reader["FullName"].ToString(),
                                    DateOfBirth = reader["DateOfBirth"] as DateTime?,
                                    Gender = reader["Gender"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    HireDate = reader["HireDate"] as DateTime?,
                                    DepartmentID = reader["DepartmentID"] as int?,
                                    PositionID = reader["PositionID"] as int?,
                                    Status = reader["Status"].ToString(),
                                    CreatedAt = reader["CreatedAt"] as DateTime?,
                                    UpdatedAt = reader["UpdatedAt"] as DateTime?,
                                    DepartmentName = reader["DepartmentName"]?.ToString(),
                                    PositionName = reader["PositionName"]?.ToString(),
                                });
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    LogError("Error in GetAllEmployees (SQL Server): ", ex);
                    throw new Exception($"Error retrieving employee list: {ex.Message}");
                }
                finally
                {
                    if (sqlConn.State == System.Data.ConnectionState.Open)
                    {
                        sqlConn.Close();
                    }
                }
            }

            using (MySqlConnection mysqlConn = new MySqlConnection(mySqlConn))
            {
                try
                {
                    mysqlConn.Open();
                    string query = "SELECT EmployeeID, DepartmentID, PositionID, Status FROM employees";
                    using (MySqlCommand cmd = new MySqlCommand(query, mysqlConn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int employeeId = Convert.ToInt32(reader["EmployeeID"]);
                            var employee = employees.FirstOrDefault(e => e.EmployeeID == employeeId);
                            if (employee != null)
                            {
                                employee.DepartmentID = reader["DepartmentID"] as int?;
                                employee.PositionID = reader["PositionID"] as int?;
                                employee.Status = reader["Status"].ToString();
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    LogError("Error in GetAllEmployees (MySQL): ", ex);
                    throw new Exception($"Error retrieving employee data from MySQL: {ex.Message}");
                }
                finally
                {
                    if (mysqlConn.State == System.Data.ConnectionState.Open)
                    {
                        mysqlConn.Close();
                    }
                }
            }
            return employees;
        }
        public void INEmailPhone(EmployeeModel ep, int? currentEmployeeId = null)
        {
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();


                string checkEmailQuery = "SELECT COUNT(*) FROM Employees WHERE Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkEmailQuery, sqlConn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", ep.Email);
                    int emailCount = (int)checkCmd.ExecuteScalar();
                    if (emailCount > 0)
                    {
                        throw new Exception("Email already exists!");
                    }
                }

                string checkPhoneQuery = "SELECT COUNT(*) FROM Employees WHERE PhoneNumber = @PhoneNumber";
                using (SqlCommand checkCmd = new SqlCommand(checkPhoneQuery, sqlConn))
                {
                    checkCmd.Parameters.AddWithValue("@PhoneNumber", ep.PhoneNumber);
                    int phoneCount = (int)checkCmd.ExecuteScalar();
                    if (phoneCount > 0)
                    {
                        throw new Exception("Phone number already exists!");
                    }
                }
            }
        }
        public void InsertNhanVien(EmployeeModel ep)
        {
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();

                // Check for duplicate email
               

                using (SqlTransaction sqlTransaction = sqlConn.BeginTransaction())
                {
                    try
                    {
                        string sqlQuery = @"
                            INSERT INTO Employees (FullName, DateOfBirth, Gender, PhoneNumber, Email, HireDate, DepartmentID, PositionID, Status, CreatedAt, UpdatedAt)
                            VALUES (@FullName, @DateOfBirth, @Gender, @PhoneNumber, @Email, @HireDate, @DepartmentID, @PositionID, @Status, @CreatedAt, @UpdatedAt);
                            SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn, sqlTransaction))
                        {
                            cmd.Parameters.AddWithValue("@FullName", ep.FullName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DateOfBirth", ep.DateOfBirth ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Gender", ep.Gender ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PhoneNumber", ep.PhoneNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Email", ep.Email ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@HireDate", ep.HireDate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DepartmentID", ep.DepartmentID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PositionID", ep.PositionID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", ep.Status ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            using (MySqlConnection mysqlConnection = new MySqlConnection(mySqlConn))
                            {
                                mysqlConnection.Open();
                                string mySqlQuery = @"
                                    INSERT INTO employees ( EmployeeID, FullName, DepartmentID, PositionID, Status )
                                    VALUES (@EmployeeID, @FullName, @DepartmentID, @PositionID, @Status)";
                                using (MySqlCommand myCmd = new MySqlCommand(mySqlQuery, mysqlConnection))
                                {
                                    myCmd.Parameters.AddWithValue("@EmployeeID", newId);
                                    myCmd.Parameters.AddWithValue("@FullName", ep.FullName ?? (object)DBNull.Value);
                                    myCmd.Parameters.AddWithValue("@DepartmentID", ep.DepartmentID ?? (object)DBNull.Value);
                                    myCmd.Parameters.AddWithValue("@PositionID", ep.PositionID ?? (object)DBNull.Value);
                                    myCmd.Parameters.AddWithValue("@Status", ep.Status ?? (object)DBNull.Value);
                                    myCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        sqlTransaction.Rollback();
                        LogError("Error in InsertNhanVien: ", ex);
                        throw new Exception("Error adding data: " + ex.Message);
                    }
                }
            }
        }

        public List<DepartmentModel> GetAllDepartments()
        {
            List<DepartmentModel> departments = new List<DepartmentModel>();
            List<int> mysqlDepartmentIds = new List<int>();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
                {
                    sqlConn.Open();
                    string query = "SELECT DepartmentID, DepartmentName, CreatedAt, UpdatedAt FROM Departments";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new DepartmentModel
                            {
                                DepartmentID = Convert.ToInt32(reader["DepartmentID"]),
                                DepartmentName = reader["DepartmentName"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                            });
                        }
                    }
                }

                using (MySqlConnection mysqlConn = new MySqlConnection(mySqlConn))
                {
                    mysqlConn.Open();
                    string query = "SELECT DepartmentID FROM departments";
                    using (MySqlCommand cmd = new MySqlCommand(query, mysqlConn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            mysqlDepartmentIds.Add(Convert.ToInt32(reader["DepartmentID"]));
                        }
                    }
                }

                departments = departments.Where(d => mysqlDepartmentIds.Contains(d.DepartmentID)).ToList();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SqlException in GetAllDepartments: {ex.Message}");
                throw new Exception($"Error retrieving department list from SQL Server: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySqlException in GetAllDepartments: {ex.Message}");
                throw new Exception($"Error retrieving department list from MySQL: {ex.Message}");
            }
            return departments;
        }


        public List<PositionsModel> GetAllPositions()
        {
            List<PositionsModel> positions = new List<PositionsModel>();
            List<int> mysqlPositionIds = new List<int>();

            try
            {
                using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
                {
                    sqlConn.Open();
                    string query = "SELECT PositionID, PositionName, CreatedAt, UpdatedAt FROM positions";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new PositionsModel
                            {
                                PositionID = Convert.ToInt32(reader["PositionID"]),
                                PositionName = reader["PositionName"].ToString(),
                                createdAt = Convert.ToDateTime(reader["CreatedAt"]),
                                updatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                            });
                        }
                    }
                }

                using (MySqlConnection mysqlConn = new MySqlConnection(mySqlConn))
                {
                    mysqlConn.Open();
                    string query = "SELECT PositionID FROM positions";
                    using (MySqlCommand cmd = new MySqlCommand(query, mysqlConn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            mysqlPositionIds.Add(Convert.ToInt32(reader["PositionID"]));
                        }
                    }
                }
                positions = positions.Where(p => mysqlPositionIds.Contains(p.PositionID)).ToList();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SqlException in GetAllPosition: {ex.Message}");
                throw new Exception($"Error retrieving position list from SQL Server: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySqlException in GetAllPosition: {ex.Message}");
                throw new Exception($"Error retrieving position list from MySQL: {ex.Message}");
            }
            return positions;
        }
        public void UpdateEmailPhone(EmployeeModel ep, int? currentEmployeeId = null)
        {
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();


                string checkEmailQuery = "SELECT COUNT(*) FROM Employees WHERE Email = @Email AND (@CurrentEmployeeID IS NULL OR EmployeeID <> @CurrentEmployeeID)";
                using (SqlCommand checkCmd = new SqlCommand(checkEmailQuery, sqlConn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", ep.Email);
                    checkCmd.Parameters.AddWithValue("@CurrentEmployeeID", (object)currentEmployeeId ?? DBNull.Value);

                    int emailCount = (int)checkCmd.ExecuteScalar();
                    if (emailCount > 0)
                    {
                        throw new Exception("Email already exists!");
                    }
                }


                string checkPhoneQuery = "SELECT COUNT(*) FROM Employees WHERE PhoneNumber = @PhoneNumber AND (@CurrentEmployeeID IS NULL OR EmployeeID <> @CurrentEmployeeID)";
                using (SqlCommand checkCmd = new SqlCommand(checkPhoneQuery, sqlConn))
                {
                    checkCmd.Parameters.AddWithValue("@PhoneNumber", ep.PhoneNumber);
                    checkCmd.Parameters.AddWithValue("@CurrentEmployeeID", (object)currentEmployeeId ?? DBNull.Value);

                    int phoneCount = (int)checkCmd.ExecuteScalar();
                    if (phoneCount > 0)
                    {
                        throw new Exception("Phone number already exists!");
                    }
                }
            }
        }
        public void UpdateNhanVien(EmployeeModel ep)
        {
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();
                using (SqlTransaction sqlTransaction = sqlConn.BeginTransaction())
                {
                    UpdateEmailPhone(ep, ep.EmployeeID);
                    try
                    {
                        string sqlQuery = @"
                            UPDATE Employees
                            SET FullName = @FullName,
                                DateOfBirth = @DateOfBirth,
                                Gender = @Gender,
                                PhoneNumber = @PhoneNumber,
                                Email = @Email,
                                HireDate = @HireDate,
                                DepartmentID = @DepartmentID,
                                PositionID = @PositionID,
                                Status = @Status,
                                UpdatedAt = @UpdatedAt
                            WHERE EmployeeID = @EmployeeID";

                        using (SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn, sqlTransaction))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", ep.EmployeeID);
                            cmd.Parameters.AddWithValue("@FullName", ep.FullName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DateOfBirth", ep.DateOfBirth ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Gender", ep.Gender ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PhoneNumber", ep.PhoneNumber ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Email", ep.Email ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@HireDate", ep.HireDate ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DepartmentID", ep.DepartmentID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PositionID", ep.PositionID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", ep.Status ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                            cmd.ExecuteNonQuery();

                            using (MySqlConnection mysqlConnection = new MySqlConnection(mySqlConn))
                            {
                                mysqlConnection.Open();
                                string mySqlQuery = @"
                                    UPDATE employees
                                    SET DepartmentID = @DepartmentID,
                                        PositionID = @PositionID,
                                        Status = @Status
                                    WHERE EmployeeID = @EmployeeID";

                                using (MySqlCommand myCmd = new MySqlCommand(mySqlQuery, mysqlConnection))
                                {
                                    myCmd.Parameters.AddWithValue("@EmployeeID", ep.EmployeeID);
                                    myCmd.Parameters.AddWithValue("@DepartmentID", ep.DepartmentID ?? (object)DBNull.Value);
                                    myCmd.Parameters.AddWithValue("@PositionID", ep.PositionID ?? (object)DBNull.Value);
                                    myCmd.Parameters.AddWithValue("@Status", ep.Status ?? (object)DBNull.Value);

                                    myCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        sqlTransaction.Rollback();
                        LogError("Error in UpdateNhanVien (Update): ", ex);
                        throw new Exception("Error updating data: " + ex.Message);
                    }
                }
            }
        }

        public void DeleteNhanVien(int employeeId)
        {
            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();
                using (SqlTransaction sqlTransaction = sqlConn.BeginTransaction())
                {
                    try
                    {
                        string sqlQuery = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn, sqlTransaction))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                            cmd.ExecuteNonQuery();

                            using (MySqlConnection mysqlConnection = new MySqlConnection(mySqlConn))
                            {
                                mysqlConnection.Open();
                                string mySqlQuery = "DELETE FROM employees WHERE EmployeeID = @EmployeeID";
                                using (MySqlCommand myCmd = new MySqlCommand(mySqlQuery, mysqlConnection))
                                {
                                    myCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                    myCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        sqlTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        sqlTransaction.Rollback();
                        LogError("Error in DeleteNhanVien: ", ex);
                        throw new Exception("Error deleting data: " + ex.Message);
                    }
                }
            }
        }


        public List<EmployeeModel> SearchEmployeesFromBothDB(string keyword)
        {
            var list = new List<EmployeeModel>();
            var employeeIds = new HashSet<int>();

            bool isNumericKeyword = int.TryParse(keyword, out int keywordAsInt);

            string sqlWhereClause = "";
            if (isNumericKeyword)
            {
                sqlWhereClause = "WHERE e.EmployeeID = @SearchParam";
            }
            else
            {
                sqlWhereClause = "WHERE e.FullName LIKE @SearchParam OR e.Email LIKE @SearchParam OR e.PhoneNumber LIKE @SearchParam";
            }

            using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
            {
                sqlConn.Open();
                string sqlQuery = $@"
            SELECT e.*, d.DepartmentName, p.PositionName
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentID = d.DepartmentID
            LEFT JOIN Positions p ON e.PositionID = p.PositionID
            {sqlWhereClause}";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn))
                {
                    if (isNumericKeyword)
                    {
                        cmd.Parameters.AddWithValue("@SearchParam", keywordAsInt);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@SearchParam", "%" + keyword + "%");
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new EmployeeModel
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                FullName = reader["FullName"].ToString(),
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                Gender = reader["Gender"]?.ToString(),
                                PhoneNumber = reader["PhoneNumber"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                HireDate = reader.IsDBNull(reader.GetOrdinal("HireDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("HireDate")),
                                DepartmentID = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? (int?)null : Convert.ToInt32(reader["DepartmentID"]),
                                PositionID = reader.IsDBNull(reader.GetOrdinal("PositionID")) ? (int?)null : Convert.ToInt32(reader["PositionID"]),
                                DepartmentName = reader["DepartmentName"]?.ToString(),
                                PositionName = reader["PositionName"]?.ToString(),
                                Status = reader["Status"]?.ToString(),
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            };

                            list.Add(employee);
                            employeeIds.Add(employee.EmployeeID);
                        }
                    }
                }
            }

            string mySqlWhereClause = "";
            if (isNumericKeyword)
            {
                mySqlWhereClause = "WHERE EmployeeID = @SearchParam";
            }
            else
            {
                mySqlWhereClause = "WHERE FullName LIKE @SearchParam ";
            }

            using (MySqlConnection myConn = new MySqlConnection(mySqlConn))
            {
                myConn.Open();
                string mysqlQuery = $"SELECT * FROM employees {mySqlWhereClause}";

                using (MySqlCommand cmd = new MySqlCommand(mysqlQuery, myConn))
                {
                    if (isNumericKeyword)
                    {
                        cmd.Parameters.AddWithValue("@SearchParam", keywordAsInt);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@SearchParam", "%" + keyword + "%");
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int empId = Convert.ToInt32(reader["EmployeeID"]);

                            if (!employeeIds.Contains(empId))
                            {
                                list.Add(new EmployeeModel
                                {
                                    EmployeeID = empId,
                                    FullName = reader["FullName"].ToString(),
                                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader["Gender"]?.ToString(),
                                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader["PhoneNumber"]?.ToString(),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader["Email"]?.ToString(),
                                    HireDate = reader.IsDBNull(reader.GetOrdinal("HireDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("HireDate")),
                                    DepartmentID = reader.IsDBNull(reader.GetOrdinal("DepartmentID")) ? (int?)null : Convert.ToInt32(reader["DepartmentID"]),
                                    PositionID = reader.IsDBNull(reader.GetOrdinal("PositionID")) ? (int?)null : Convert.ToInt32(reader["PositionID"]),
                                    DepartmentName = null,
                                    PositionName = null,
                                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader["Status"]?.ToString(),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                                });
                            }
                        }
                    }
                }
            }

            return list;
        }

        public bool EmployeeExists(int employeeId)
        {
            using (SqlConnection conn = new SqlConnection(sqlServerConn))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Employees WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in EmployeeDAL.EmployeeExists: {ex.Message}");
                    throw;
                }
            }
        }
        public string GetEmployeeEmailById(int employeeId)
        {
            string email = null;
            string sql = "SELECT Email FROM Employees WHERE EmployeeID = @EmployeeID";

            using (SqlConnection conn = new SqlConnection(sqlServerConn))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employeeId;
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            email = result.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in EmployeeDAL.GetEmployeeEmailById: {ex.Message}");
                    throw;
                }
            }
            return email;
        }

        private void LogError(string message, Exception ex)
        {
            Debug.WriteLine(message);
            Debug.WriteLine(ex.ToString());
        }
    }
}