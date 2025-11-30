using Npgsql;
using UD_WForms.Models;
using UD_WForms.Models.Database;
using System.Collections.Generic;
using System;

namespace UD_WForms.Services
{
    public class FlightService : IFlightService
    {
        public List<Flight> GetAllFlights()
        {
            var flights = new List<Flight>();

            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    FlightNumber, FlightType, Aircraft, DepartureDate, ArrivalDate, 
                    FlightTime, Status, DepartureAirportId, ArrivalAirportId, 
                    EconomySeats, BusinessSeats, Airline
                FROM Flight 
                ORDER BY DepartureDate";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var flight = new Flight
                                {
                                    FlightNumber = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                    FlightType = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    Aircraft = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    DepartureDate = reader.IsDBNull(3) ? DateTime.Now : reader.GetDateTime(3),
                                    ArrivalDate = reader.IsDBNull(4) ? DateTime.Now : reader.GetDateTime(4),
                                    FlightTime = reader.IsDBNull(5) ? TimeSpan.Zero : reader.GetTimeSpan(5),
                                    Status = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    DepartureAirportId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                    ArrivalAirportId = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                                    EconomySeats = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                                    BusinessSeats = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                                    Airline = reader.IsDBNull(11) ? "" : reader.GetString(11)
                                };
                                flights.Add(flight);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка чтения данных рейса: {ex.Message}");
                            }
                        }
                    }
                }

                Console.WriteLine($"Успешно загружено {flights.Count} рейсов");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetAllFlights: {ex.Message}");
                throw new Exception($"Ошибка получения рейсов: {ex.Message}");
            }

            return flights;
        }

        // ... остальные методы пока можно оставить пустыми для теста
        public Flight GetFlightByNumber(string flightNumber)
        {
            if (string.IsNullOrEmpty(flightNumber))
                return null;

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = @"
            SELECT FlightNumber, FlightType, Aircraft, DepartureDate, ArrivalDate, 
                   FlightTime, Status, DepartureAirportId, ArrivalAirportId, 
                   EconomySeats, BusinessSeats, Airline
            FROM Flight 
            WHERE FlightNumber = @FlightNumber";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FlightNumber", flightNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Flight
                            {
                                FlightNumber = reader.GetString(0),
                                FlightType = reader.GetString(1),
                                Aircraft = reader.GetString(2),
                                DepartureDate = reader.GetDateTime(3),
                                ArrivalDate = reader.GetDateTime(4),
                                FlightTime = reader.GetTimeSpan(5),
                                Status = reader.GetString(6),
                                DepartureAirportId = reader.GetInt32(7),
                                ArrivalAirportId = reader.GetInt32(8),
                                EconomySeats = reader.GetInt32(9),
                                BusinessSeats = reader.GetInt32(10),
                                Airline = reader.GetString(11)
                            };
                        }
                    }
                }
            }
            return null;
        }
        public bool CreateFlight(Flight flight) 
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();

                    // Добавим отладочную информацию
                    Console.WriteLine($"Создание рейса: {flight.FlightNumber}");
                    Console.WriteLine($"Аэропорт вылета: {flight.DepartureAirportId}, Прибытия: {flight.ArrivalAirportId}");

                    string query = @"
                INSERT INTO Flight (
                    FlightNumber, FlightType, Aircraft, DepartureDate, ArrivalDate, 
                    FlightTime, Status, DepartureAirportId, ArrivalAirportId, 
                    EconomySeats, BusinessSeats, Airline
                ) VALUES (
                    @FlightNumber, @FlightType, @Aircraft, @DepartureDate, @ArrivalDate,
                    @FlightTime, @Status, @DepartureAirportId, @ArrivalAirportId,
                    @EconomySeats, @BusinessSeats, @Airline
                )";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FlightNumber", flight.FlightNumber ?? "");
                        cmd.Parameters.AddWithValue("@FlightType", flight.FlightType ?? "Регулярный");
                        cmd.Parameters.AddWithValue("@Aircraft", flight.Aircraft ?? "");
                        cmd.Parameters.AddWithValue("@DepartureDate", flight.DepartureDate);
                        cmd.Parameters.AddWithValue("@ArrivalDate", flight.ArrivalDate);
                        cmd.Parameters.AddWithValue("@FlightTime", flight.FlightTime);
                        cmd.Parameters.AddWithValue("@Status", flight.Status ?? "По расписанию");
                        cmd.Parameters.AddWithValue("@DepartureAirportId", flight.DepartureAirportId);
                        cmd.Parameters.AddWithValue("@ArrivalAirportId", flight.ArrivalAirportId);
                        cmd.Parameters.AddWithValue("@EconomySeats", flight.EconomySeats);
                        cmd.Parameters.AddWithValue("@BusinessSeats", flight.BusinessSeats);
                        cmd.Parameters.AddWithValue("@Airline", flight.Airline ?? "");

                        int result = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Результат выполнения: {result} строк affected");

                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании рейса: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Ошибка при создании рейса: {ex.Message}");
            }
        }

        public bool UpdateFlight(Flight flight) => false;
        public bool DeleteFlight(string flightNumber) => false;
        public List<Flight> SearchFlights(string searchTerm) => new List<Flight>();
        public List<Flight> GetFlightsByAirport(int airportId) => new List<Flight>();

    }
}