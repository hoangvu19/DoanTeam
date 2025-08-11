using System;
using System.Configuration;
using System.Data.SqlClient;
using DoAnTeam12.Models.Account;
using BCrypt.Net;

namespace DoAnTeam12.DAL
{
    public class AccountDAL
    {
        private readonly string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;

        public string AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(sqlServerConn))
                {
                    conn.Open();
                    string query = "SELECT Password, IsActive, Role FROM [HUMAN].[dbo].[Account] WHERE UserName = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPasswordHash = reader["Password"].ToString();
                                bool isActive = (bool)reader["IsActive"];
                                string role = reader["Role"].ToString();

                                if (!isActive)
                                    return null;

                                
                                if (!BCrypt.Net.BCrypt.Verify(password, storedPasswordHash))
                                    return null;

                                return role;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AccountDAL.AuthenticateUser: {ex.Message}");
                throw;
            }
        }

        public bool IsUserNameExists(string userName)
        {
            using (SqlConnection conn = new SqlConnection(sqlServerConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM [HUMAN].[dbo].[Account] WHERE UserName = @UserName";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool IsEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            using (SqlConnection conn = new SqlConnection(sqlServerConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM [HUMAN].[dbo].[Account] WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool IsMobileExists(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return false;

            using (SqlConnection conn = new SqlConnection(sqlServerConn))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM [HUMAN].[dbo].[Account] WHERE Mobile = @Mobile";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Mobile", mobile);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public void CreateUser(UserModel userModel)
        {
            if (userModel == null)
                throw new ArgumentNullException(nameof(userModel), "User information cannot be null.");

            try
            {
                using (SqlConnection conn = new SqlConnection(sqlServerConn))
                {
                    conn.Open();
                    string query = @"INSERT INTO [HUMAN].[dbo].[Account]  
                                     (UserName, Password, Email, Mobile, IsActive, IsRemember, Role)  
                                     VALUES (@UserName, @Password, @Email, @Mobile, @IsActive, @IsRemember, @Role)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userModel.Password);

                        cmd.Parameters.AddWithValue("@UserName", userModel.UserName);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Email", (object)userModel.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Mobile", (object)userModel.Mobile ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", userModel.IsActive);
                        cmd.Parameters.AddWithValue("@IsRemember", userModel.IsRemember);
                        cmd.Parameters.AddWithValue("@Role", userModel.Role);

                        int result = cmd.ExecuteNonQuery();

                        if (result <= 0)
                        {
                            throw new Exception("Failed to create new user.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in AccountDAL.CreateUser: {ex.Message}");
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Username or Email already exists.");
                }
                throw;
            }
        }
        public bool ResetPassword(string email, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("Email and new password cannot be empty.");

            try
            {
                using (SqlConnection conn = new SqlConnection(sqlServerConn))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM [HUMAN].[dbo].[Account] WHERE Email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        if (count == 0)
                        {
                            return false;
                        }
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    string updateQuery = "UPDATE [HUMAN].[dbo].[Account] SET Password = @Password WHERE Email = @Email";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        updateCmd.Parameters.AddWithValue("@Email", email);
                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AccountDAL.ResetPassword: {ex.Message}");
                throw;
            }
        }

    }
}
