using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}