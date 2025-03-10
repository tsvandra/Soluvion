using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Soluvion.Models;

namespace Soluvion.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password); // Később jelszó hash-elést kell használni

                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<User> GetUserAsync(string username)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT u.Id, u.Username, u.Password, u.Name, u.RoleId, r.RoleName, r.Description
                    FROM Users u
                    JOIN UserRoles r ON u.RoleId = r.Id
                    WHERE u.Username = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Name = reader["Name"].ToString(),
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                Role = new UserRole
                                {
                                    Id = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString(),
                                    Description = reader["Description"].ToString()
                                }
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            Dictionary<int, UserRole> roles = new Dictionary<int, UserRole>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, RoleName, Description FROM UserRoles";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int roleId = Convert.ToInt32(reader["Id"]);
                            roles.TryAdd(roleId, new UserRole
                            {
                                Id = roleId,
                                RoleName = reader["RoleName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }

                string usersQuery = "SELECT Id, Username, Password, Name, RoleId FROM Users";

                using (SqlCommand command = new SqlCommand(usersQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int roleId = Convert.ToInt32(reader["RoleId"]);

                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Name = reader["Name"].ToString(),
                                RoleId = roleId,
                                Role = roles.ContainsKey(roleId) ? roles[roleId] : null
                            });
                        }
                    }
                }
            }

            return users;
        }

        public async Task<bool> CheckUserExistsAsync(string username)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"INSERT INTO Users (Username, Password, Name, RoleId) 
                        VALUES (@Username, @Password, @Name, @RoleId)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@RoleId", user.RoleId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<UserRole>> GetAllUserRolesAsync()
        {
            List<UserRole> roles = new List<UserRole>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, RoleName, Description FROM UserRoles";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(new UserRole
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                RoleName = reader["RoleName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }
            }

            return roles;
        }
    }
}