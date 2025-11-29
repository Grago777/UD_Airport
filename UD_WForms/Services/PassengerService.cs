using Npgsql;
using UD_WForms.Models;
using UD_WForms.Models.Database;
using System.Collections.Generic;
using System;

namespace UD_WForms.Services
{
    public class PassengerService : IPassengerService
    {
        public List<Passenger> GetAllPassengers()
        {
            var passengers = new List<Passenger>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Passenger ORDER BY PassengerId";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        passengers.Add(new Passenger
                        {
                            PassengerId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                            PassportData = reader.GetString(4)
                        });
                    }
                }
            }

            return passengers;
        }

        public Passenger GetPassengerById(int passengerId)
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Passenger WHERE PassengerId = @PassengerId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@PassengerId", passengerId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Passenger
                            {
                                PassengerId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PassportData = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool CreatePassenger(Passenger passenger)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Passenger (FullName, PhoneNumber, Email, PassportData)
                        VALUES (@FullName, @PhoneNumber, @Email, @PassportData)";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FullName", passenger.FullName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", (object)passenger.PhoneNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", (object)passenger.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PassportData", passenger.PassportData);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании пассажира: {ex.Message}");
            }
        }

        public bool UpdatePassenger(Passenger passenger)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        UPDATE Passenger 
                        SET FullName = @FullName, PhoneNumber = @PhoneNumber, 
                            Email = @Email, PassportData = @PassportData
                        WHERE PassengerId = @PassengerId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PassengerId", passenger.PassengerId);
                        cmd.Parameters.AddWithValue("@FullName", passenger.FullName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", (object)passenger.PhoneNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Email", (object)passenger.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PassportData", passenger.PassportData);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении пассажира: {ex.Message}");
            }
        }

        public bool DeletePassenger(int passengerId)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Passenger WHERE PassengerId = @PassengerId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PassengerId", passengerId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении пассажира: {ex.Message}");
            }
        }

        public List<Passenger> SearchPassengers(string searchTerm)
        {
            var passengers = new List<Passenger>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = @"
                    SELECT * FROM Passenger 
                    WHERE FullName ILIKE @SearchTerm 
                       OR PhoneNumber ILIKE @SearchTerm
                       OR Email ILIKE @SearchTerm
                       OR PassportData ILIKE @SearchTerm
                    ORDER BY PassengerId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passengers.Add(new Passenger
                            {
                                PassengerId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                PhoneNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PassportData = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return passengers;
        }
    }
}