using Npgsql;
using UD_WForms.Models;
using UD_WForms.Models.Database;
using System.Collections.Generic;
using System;

namespace UD_WForms.Services
{
    public class AirportService : IAirportService
    {
        public List<Airport> GetAllAirports()
        {
            var airports = new List<Airport>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Airport ORDER BY Country, City, Name";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        airports.Add(new Airport
                        {
                            AirportId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IATACode = reader.GetString(2),
                            Country = reader.GetString(3),
                            City = reader.GetString(4)
                        });
                    }
                }
            }

            return airports;
        }

        public Airport GetAirportById(int airportId)
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Airport WHERE AirportId = @AirportId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AirportId", airportId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Airport
                            {
                                AirportId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                IATACode = reader.GetString(2),
                                Country = reader.GetString(3),
                                City = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Airport GetAirportByIATACode(string iataCode)
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Airport WHERE IATACode = @IATACode";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@IATACode", iataCode);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Airport
                            {
                                AirportId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                IATACode = reader.GetString(2),
                                Country = reader.GetString(3),
                                City = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool CreateAirport(Airport airport)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Airport (Name, IATACode, Country, City)
                        VALUES (@Name, @IATACode, @Country, @City)";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", airport.Name);
                        cmd.Parameters.AddWithValue("@IATACode", airport.IATACode);
                        cmd.Parameters.AddWithValue("@Country", airport.Country);
                        cmd.Parameters.AddWithValue("@City", airport.City);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании аэропорта: {ex.Message}");
            }
        }

        public bool UpdateAirport(Airport airport)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        UPDATE Airport 
                        SET Name = @Name, IATACode = @IATACode, 
                            Country = @Country, City = @City
                        WHERE AirportId = @AirportId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@AirportId", airport.AirportId);
                        cmd.Parameters.AddWithValue("@Name", airport.Name);
                        cmd.Parameters.AddWithValue("@IATACode", airport.IATACode);
                        cmd.Parameters.AddWithValue("@Country", airport.Country);
                        cmd.Parameters.AddWithValue("@City", airport.City);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении аэропорта: {ex.Message}");
            }
        }

        public bool DeleteAirport(int airportId)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Airport WHERE AirportId = @AirportId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@AirportId", airportId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении аэропорта: {ex.Message}");
            }
        }

        public List<Airport> SearchAirports(string searchTerm)
        {
            var airports = new List<Airport>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Airport 
                    WHERE Name ILIKE @SearchTerm 
                       OR IATACode ILIKE @SearchTerm
                       OR Country ILIKE @SearchTerm
                       OR City ILIKE @SearchTerm
                    ORDER BY Country, City, Name";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            airports.Add(new Airport
                            {
                                AirportId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                IATACode = reader.GetString(2),
                                Country = reader.GetString(3),
                                City = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return airports;
        }
    }
}