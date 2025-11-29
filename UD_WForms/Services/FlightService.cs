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
                        SELECT f.FlightNumber, f.FlightType, f.Aircraft, f.DepartureDate, 
                               f.ArrivalDate, f.FlightTime, f.Status, f.DepartureAirportId, 
                               f.ArrivalAirportId, f.EconomySeats, f.BusinessSeats, f.Airline
                        FROM Flight f
                        ORDER BY f.DepartureDate";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flights.Add(new Flight
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
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения рейсов: {ex.Message}");
            }

            return flights;
        }

        // ... остальные методы пока можно оставить пустыми для теста
        public Flight GetFlightByNumber(string flightNumber) => null;
        public bool CreateFlight(Flight flight) => false;
        public bool UpdateFlight(Flight flight) => false;
        public bool DeleteFlight(string flightNumber) => false;
        public List<Flight> SearchFlights(string searchTerm) => new List<Flight>();
        public List<Flight> GetFlightsByAirport(int airportId) => new List<Flight>();
    }
}