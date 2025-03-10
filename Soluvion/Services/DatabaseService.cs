using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Soluvion.Models;

namespace Soluvion.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = MauiProgram.ConnectionString;
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

        // Ellenőrzi, hogy létezik-e már felhasználó az adott felhasználónévvel
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

        // Új felhasználó létrehozása
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

        // Szerepkörök lekérdezése
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

        // Időpontfoglalások lekérdezése egy adott dolgozó számára
        public async Task<List<Models.Appointment>> GetAppointmentsForEmployeeAsync(int employeeId)
        {
            List<Models.Appointment> appointments = new List<Models.Appointment>();
            Dictionary<int, User> users = new Dictionary<int, User>();
            Dictionary<int, Service> services = new Dictionary<int, Service>();
            Dictionary<int, AppointmentStatus> statuses = new Dictionary<int, AppointmentStatus>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Először betöltjük a kapcsolódó adatokat
                // 1. Státuszok betöltése
                string statusQuery = "SELECT Id, StatusName, Description FROM AppointmentStatuses";
                using (SqlCommand command = new SqlCommand(statusQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            statuses[id] = new AppointmentStatus
                            {
                                Id = id,
                                StatusName = reader["StatusName"].ToString(),
                                Description = reader["Description"].ToString()
                            };
                        }
                    }
                }

                // 2. Szolgáltatások betöltése
                string serviceQuery = "SELECT s.Id, s.Name, s.Description, s.Duration, s.Price, s.SalonId, " +
                                     "sa.Name AS SalonName, sa.Address, sa.Phone, sa.Email " +
                                     "FROM Services s JOIN Salons sa ON s.SalonId = sa.Id";
                using (SqlCommand command = new SqlCommand(serviceQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            services[id] = new Service
                            {
                                Id = id,
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Duration = Convert.ToInt32(reader["Duration"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                SalonId = Convert.ToInt32(reader["SalonId"]),
                                Salon = new Salon
                                {
                                    Id = Convert.ToInt32(reader["SalonId"]),
                                    Name = reader["SalonName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Email = reader["Email"].ToString()
                                }
                            };
                        }
                    }
                }

                // 3. Felhasználók betöltése
                string userQuery = "SELECT u.Id, u.Username, u.Name, u.RoleId, r.RoleName " +
                                  "FROM Users u JOIN UserRoles r ON u.RoleId = r.Id";
                using (SqlCommand command = new SqlCommand(userQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            users[id] = new User
                            {
                                Id = id,
                                Username = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                Role = new UserRole
                                {
                                    Id = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString()
                                }
                            };
                        }
                    }
                }

                // 4. Időpontok lekérdezése
                string appointmentQuery = "SELECT Id, CustomerId, ServiceId, EmployeeId, AppointmentDate, " +
                                         "StatusId, Notes, CreatedAt FROM Appointments " +
                                         "WHERE EmployeeId = @EmployeeId OR EmployeeId IS NULL " +
                                         "ORDER BY AppointmentDate DESC";

                using (SqlCommand command = new SqlCommand(appointmentQuery, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", employeeId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int customerId = Convert.ToInt32(reader["CustomerId"]);
                            int serviceId = Convert.ToInt32(reader["ServiceId"]);
                            int statusId = Convert.ToInt32(reader["StatusId"]);
                            object employeeIdObj = reader["EmployeeId"];
                            int? empId = employeeIdObj == DBNull.Value ? null : Convert.ToInt32(employeeIdObj);

                            appointments.Add(new Models.Appointment
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CustomerId = customerId,
                                Customer = users.ContainsKey(customerId) ? users[customerId] : null,
                                ServiceId = serviceId,
                                Service = services.ContainsKey(serviceId) ? services[serviceId] : null,
                                EmployeeId = empId,
                                Employee = empId.HasValue && users.ContainsKey(empId.Value) ? users[empId.Value] : null,
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                StatusId = statusId,
                                AppointmentStatus = statuses.ContainsKey(statusId) ? statuses[statusId] : null, // Használjuk az AppointmentStatus tulajdonságot
                                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }

            return appointments;
        }

        // Időpontfoglalás státuszának frissítése
        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, int statusId, int employeeId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "UPDATE Appointments SET StatusId = @StatusId, EmployeeId = @EmployeeId " +
                              "WHERE Id = @AppointmentId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StatusId", statusId);
                    command.Parameters.AddWithValue("@EmployeeId", employeeId);
                    command.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        // Összes státusz lekérdezése
        public async Task<List<AppointmentStatus>> GetAllAppointmentStatusesAsync()
        {
            List<AppointmentStatus> statuses = new List<AppointmentStatus>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, StatusName, Description FROM AppointmentStatuses";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            statuses.Add(new AppointmentStatus
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                StatusName = reader["StatusName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }
            }

            return statuses;
        }

        public async Task<bool> CreateAppointmentAsync(Appointment appointment)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"INSERT INTO Appointments (CustomerId, ServiceId, EmployeeId, AppointmentDate, StatusId, Notes, CreatedAt) 
                        VALUES (@CustomerId, @ServiceId, @EmployeeId, @AppointmentDate, @StatusId, @Notes, @CreatedAt)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
                    command.Parameters.AddWithValue("@ServiceId", appointment.ServiceId);
                    command.Parameters.AddWithValue("@EmployeeId", (object)appointment.EmployeeId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                    command.Parameters.AddWithValue("@StatusId", appointment.StatusId);
                    command.Parameters.AddWithValue("@Notes", (object)appointment.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", appointment.CreatedAt);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            List<Service> services = new List<Service>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT s.Id, s.Name, s.Description, s.Duration, s.Price, s.SalonId, " +
                               "sa.Name AS SalonName, sa.Address, sa.Phone, sa.Email " +
                               "FROM Services s JOIN Salons sa ON s.SalonId = sa.Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            services.Add(new Service
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Duration = Convert.ToInt32(reader["Duration"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                SalonId = Convert.ToInt32(reader["SalonId"]),
                                Salon = new Salon
                                {
                                    Id = Convert.ToInt32(reader["SalonId"]),
                                    Name = reader["SalonName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Email = reader["Email"].ToString()
                                }
                            });
                        }
                    }
                }
            }

            return services;
        }

        public async Task<List<Appointment>> GetAppointmentsForCustomerAsync(int customerId)
        {
            List<Appointment> appointments = new List<Appointment>();
            Dictionary<int, User> users = new Dictionary<int, User>();
            Dictionary<int, Service> services = new Dictionary<int, Service>();
            Dictionary<int, AppointmentStatus> statuses = new Dictionary<int, AppointmentStatus>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Először betöltjük a kapcsolódó adatokat
                // 1. Státuszok betöltése
                string statusQuery = "SELECT Id, StatusName, Description FROM AppointmentStatuses";
                using (SqlCommand command = new SqlCommand(statusQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            statuses[id] = new AppointmentStatus
                            {
                                Id = id,
                                StatusName = reader["StatusName"].ToString(),
                                Description = reader["Description"].ToString()
                            };
                        }
                    }
                }

                // 2. Szolgáltatások betöltése
                string serviceQuery = "SELECT s.Id, s.Name, s.Description, s.Duration, s.Price, s.SalonId, " +
                                     "sa.Name AS SalonName, sa.Address, sa.Phone, sa.Email " +
                                     "FROM Services s JOIN Salons sa ON s.SalonId = sa.Id";
                using (SqlCommand command = new SqlCommand(serviceQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            services[id] = new Service
                            {
                                Id = id,
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Duration = Convert.ToInt32(reader["Duration"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                SalonId = Convert.ToInt32(reader["SalonId"]),
                                Salon = new Salon
                                {
                                    Id = Convert.ToInt32(reader["SalonId"]),
                                    Name = reader["SalonName"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Email = reader["Email"].ToString()
                                }
                            };
                        }
                    }
                }

                // 3. Felhasználók betöltése
                string userQuery = "SELECT u.Id, u.Username, u.Name, u.RoleId, r.RoleName " +
                                  "FROM Users u JOIN UserRoles r ON u.RoleId = r.Id";
                using (SqlCommand command = new SqlCommand(userQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            users[id] = new User
                            {
                                Id = id,
                                Username = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                Role = new UserRole
                                {
                                    Id = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString()
                                }
                            };
                        }
                    }
                }

                // 4. Időpontok lekérdezése
                string appointmentQuery = "SELECT Id, CustomerId, ServiceId, EmployeeId, AppointmentDate, " +
                                         "StatusId, Notes, CreatedAt FROM Appointments " +
                                         "WHERE CustomerId = @CustomerId " +
                                         "ORDER BY AppointmentDate DESC";

                using (SqlCommand command = new SqlCommand(appointmentQuery, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int serviceId = Convert.ToInt32(reader["ServiceId"]);
                            int statusId = Convert.ToInt32(reader["StatusId"]);
                            object employeeIdObj = reader["EmployeeId"];
                            int? empId = employeeIdObj == DBNull.Value ? null : Convert.ToInt32(employeeIdObj);

                            appointments.Add(new Appointment
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CustomerId = customerId,
                                Customer = users.ContainsKey(customerId) ? users[customerId] : null,
                                ServiceId = serviceId,
                                Service = services.ContainsKey(serviceId) ? services[serviceId] : null,
                                EmployeeId = empId,
                                Employee = empId.HasValue && users.ContainsKey(empId.Value) ? users[empId.Value] : null,
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                StatusId = statusId,
                                //Status = statuses.ContainsKey(statusId) ? statuses[statusId] : null,
                                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }

            return appointments;
        }
    }
}

