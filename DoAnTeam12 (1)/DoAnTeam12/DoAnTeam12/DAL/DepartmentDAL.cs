using DoAnTeam12.Models.Department;
using DoAnTeam12.Models.Employee;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace DoAnTeam12.DAL
{
    public class DepartmentDAL
    {
        private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"]?.ConnectionString;
        private string mySqlConn = ConfigurationManager.ConnectionStrings["MySqlConnection"]?.ConnectionString;

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

                // Filter out departments that are not in MySQL
                departments = departments.Where(d => mysqlDepartmentIds.Contains(d.DepartmentID)).ToList();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SqlException in GetAllDepartments: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách phòng ban từ SQL Server: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySqlException in GetAllDepartments: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách phòng ban từ MySQL: {ex.Message}");
            }
            return departments;
        }


        public List<EmployeeModel> GetEmployeesByDepartment(int departmentId)
        {
            List<EmployeeModel> employees = new List<EmployeeModel>();
            bool departmentExistsInMySql = false;

            try
            {
                using (MySqlConnection mysqlConn = new MySqlConnection(mySqlConn))
                {
                    mysqlConn.Open();
                    string checkDeptQuery = "SELECT COUNT(*) FROM departments WHERE DepartmentID = @DepartmentID";
                    using (MySqlCommand cmd = new MySqlCommand(checkDeptQuery, mysqlConn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            departmentExistsInMySql = true;
                        }
                    }
                }

                if (departmentExistsInMySql)
                {
                    using (SqlConnection sqlConn = new SqlConnection(sqlServerConn))
                    {
                        sqlConn.Open();
                        string query = @"
                            SELECT 
                                e.EmployeeID, 
                                e.FullName, 
                                e.DateOfBirth, 
                                e.Gender, 
                                e.PhoneNumber, 
                                e.Email, 
                                e.HireDate, 
                                e.DepartmentID, 
                                d.DepartmentName, 
                                e.PositionID, 
                                p.PositionName, 
                                e.Status, 
                                e.CreatedAt, 
                                e.UpdatedAt
                            FROM Employees e
                            JOIN Departments d ON e.DepartmentID = d.DepartmentID
                            JOIN Positions p ON e.PositionID = p.PositionID
                            WHERE e.DepartmentID = @DepartmentID";

                        using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                        {
                            cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    employees.Add(new EmployeeModel
                                    {
                                        EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                        FullName = reader["FullName"].ToString(),
                                        DateOfBirth = reader["DateOfBirth"] as DateTime?,
                                        Gender = reader["Gender"].ToString(),
                                        PhoneNumber = reader["PhoneNumber"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        HireDate = reader["HireDate"] as DateTime?,
                                        DepartmentID = reader["DepartmentID"] as int?,
                                        DepartmentName = reader["DepartmentName"].ToString(),
                                        PositionID = reader["PositionID"] as int?,
                                        PositionName = reader["PositionName"].ToString(),
                                        Status = reader["Status"].ToString(),
                                        CreatedAt = reader["CreatedAt"] as DateTime?,
                                        UpdatedAt = reader["UpdatedAt"] as DateTime?
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Department with ID {departmentId} does not exist in MySQL. No employees fetched from SQL Server.");
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SqlException in GetEmployeesByDepartment: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách nhân viên từ SQL Server: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySqlException in GetEmployeesByDepartment: {ex.Message}");
                throw new Exception($"Lỗi khi kiểm tra phòng ban trong MySQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Exception in GetEmployeesByDepartment: {ex.Message}");
                throw new Exception($"Đã xảy ra lỗi không xác định khi lấy danh sách nhân viên: {ex.Message}");
            }
            return employees;
        }
        public List<PositionsModel> GetAllPosition()
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
                throw new Exception($"Lỗi khi lấy danh sách vị trí từ SQL Server: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySqlException in GetAllPosition: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách vị trí từ MySQL: {ex.Message}");
            }
            return positions;
        }

       
    }
}