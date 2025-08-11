using DoAnTeam12.Models.Account;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;

namespace DoAnTeam12.DAL
{
    public class AuthDAL
    {
        private string sqlServerConn = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;

        public AuthDAL()
        {

        }

        public List<UserModel> GetTotal()
        {
            List<UserModel> userList = new List<UserModel>();
            SqlConnection conn = null;

            string query = "SELECT Id, UserName, Email, Mobile, Password, IsActive, IsRemember, Role FROM Account";

            try
            {
                conn = new SqlConnection(sqlServerConn);
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {

                            int idOrdinal = reader.GetOrdinal("Id");
                            int userNameOrdinal = reader.GetOrdinal("UserName");
                            int emailOrdinal = reader.GetOrdinal("Email");
                            int mobileOrdinal = reader.GetOrdinal("Mobile");
                            int passwordOrdinal = reader.GetOrdinal("Password");
                            int isActiveOrdinal = reader.GetOrdinal("IsActive");
                            int isRememberOrdinal = reader.GetOrdinal("IsRemember");
                            int roleOrdinal = reader.GetOrdinal("Role");

                            while (reader.Read())
                            {
                                UserModel user = new UserModel();

                                user.Id = reader.GetInt32(idOrdinal);
                                user.UserName = reader.IsDBNull(userNameOrdinal) ? string.Empty : reader.GetString(userNameOrdinal);
                                user.Password = reader.IsDBNull(passwordOrdinal) ? string.Empty : reader.GetString(passwordOrdinal);
                                user.Email = reader[emailOrdinal] as string;
                                user.Mobile = reader.IsDBNull(mobileOrdinal) ? null : reader.GetValue(mobileOrdinal).ToString();
                                user.IsActive = reader.GetBoolean(isActiveOrdinal);
                                user.IsRemember = reader.GetBoolean(isRememberOrdinal);
                                user.Role = reader[roleOrdinal] as string;

                                userList.Add(user);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                LogError("SQL Error in AuthDAL.GetTotal: ", sqlEx);
                throw new Exception($"Error retrieving account list from SQL Server: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                LogError("General Error in AuthDAL.GetTotal: ", ex);
                throw new Exception($"General error retrieving account list: {ex.Message}", ex);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return userList;
        }



        public UserModel GetById(int id)
        {
            UserModel user = null;
            SqlConnection conn = null;

            string query = "SELECT Id, UserName, Email, Mobile, Password, IsActive, IsRemember, Role FROM Account WHERE Id = @Id";

            try
            {
                conn = new SqlConnection(sqlServerConn);
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {

                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            int idOrdinal = reader.GetOrdinal("Id");
                            int userNameOrdinal = reader.GetOrdinal("UserName");
                            int emailOrdinal = reader.GetOrdinal("Email");
                            int mobileOrdinal = reader.GetOrdinal("Mobile");
                            int passwordOrdinal = reader.GetOrdinal("Password");
                            int isActiveOrdinal = reader.GetOrdinal("IsActive");
                            int isRememberOrdinal = reader.GetOrdinal("IsRemember");
                            int roleOrdinal = reader.GetOrdinal("Role");

                            user = new UserModel();


                            user.Id = reader.GetInt32(idOrdinal);
                            user.UserName = reader.IsDBNull(userNameOrdinal) ? string.Empty : reader.GetString(userNameOrdinal);
                            user.Password = reader.IsDBNull(passwordOrdinal) ? string.Empty : reader.GetString(passwordOrdinal);
                            user.Email = reader[emailOrdinal] as string;
                            user.Mobile = reader.IsDBNull(mobileOrdinal) ? null : reader.GetValue(mobileOrdinal).ToString();
                            user.IsActive = reader.GetBoolean(isActiveOrdinal);
                            user.IsRemember = reader.GetBoolean(isRememberOrdinal);
                            user.Role = reader[roleOrdinal] as string;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                LogError($"SQL Error in AuthDAL.GetById (ID: {id}): ", sqlEx);
                throw new Exception($"Error retrieving account by ID from SQL Server: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                LogError($"General Error in AuthDAL.GetById (ID: {id}): ", ex);
                throw new Exception($"General error retrieving account by ID: {ex.Message}", ex);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return user;
        }


        public void UpdateAccount(UserModel account)
        {
            SqlConnection conn = null;

            string query = @"
                    UPDATE Account
                    SET UserName = @UserName,
                        Email = @Email,
                        Mobile = @Mobile,
                        Password = @Password,
                        IsActive = @IsActive,
                        IsRemember = @IsRemember,
                        Role = @Role
                    WHERE Id = @Id";

            try
            {
                conn = new SqlConnection(sqlServerConn);
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {

                    cmd.Parameters.AddWithValue("@Id", account.Id);
                    cmd.Parameters.AddWithValue("@UserName", account.UserName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", account.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Mobile", account.Mobile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Password", account.Password ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", account.IsActive);
                    cmd.Parameters.AddWithValue("@IsRemember", account.IsRemember);
                    cmd.Parameters.AddWithValue("@Role", account.Role ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                LogError($"SQL Error in AuthDAL.UpdateAccount (ID: {account.Id}): ", sqlEx);
                throw new Exception($"Error updating account on SQL Server: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                LogError($"General Error in AuthDAL.UpdateAccount (ID: {account.Id}): ", ex);
                throw new Exception($"General error updating account: {ex.Message}", ex);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public void DeleteAccount(int id)
        {
            SqlConnection conn = null;

            string query = "DELETE FROM Account WHERE Id = @Id";

            try
            {
                conn = new SqlConnection(sqlServerConn);
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                LogError($"SQL Error in AuthDAL.DeleteAccount (ID: {id}): ", sqlEx);
                throw new Exception($"Error deleting account on SQL Server: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                LogError($"General Error in AuthDAL.DeleteAccount (ID: {id}): ", ex);
                throw new Exception($"General error deleting account: {ex.Message}", ex);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }


        private void LogError(string message, Exception ex = null)
        {
            Debug.WriteLine(message);
            if (ex != null)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
