using Npgsql;
using UD_WForms.Models;
using UD_WForms.Models.Database;
using System.Collections.Generic;
using System;

namespace UD_WForms.Services
{
    public class TicketService : ITicketService
    {
        public List<Ticket> GetAllTickets()
        {
            var tickets = new List<Ticket>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = @"
                    SELECT t.RecordNumber, t.TicketNumber, t.FlightNumber, t.PassengerId, 
                           t.Class, t.Status, t.Luggage, t.Price,
                           p.FullName, f.DepartureDate, f.ArrivalDate,
                           dep.Name as DepartureAirport, arr.Name as ArrivalAirport
                    FROM Ticket t
                    JOIN Passenger p ON t.PassengerId = p.PassengerId
                    JOIN Flight f ON t.FlightNumber = f.FlightNumber
                    JOIN Airport dep ON f.DepartureAirportId = dep.AirportId
                    JOIN Airport arr ON f.ArrivalAirportId = arr.AirportId
                    ORDER BY t.RecordNumber";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tickets.Add(new Ticket
                        {
                            RecordNumber = reader.GetInt32(0),
                            TicketNumber = reader.GetString(1),
                            FlightNumber = reader.GetString(2),
                            PassengerId = reader.GetInt32(3),
                            Class = reader.GetString(4),
                            Status = reader.GetString(5),
                            Luggage = reader.GetDecimal(6),
                            Price = reader.GetDecimal(7)
                        });
                    }
                }
            }

            return tickets;
        }

        public Ticket GetTicketById(int recordNumber)
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Ticket WHERE RecordNumber = @RecordNumber";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@RecordNumber", recordNumber);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Ticket
                            {
                                RecordNumber = reader.GetInt32(0),
                                TicketNumber = reader.GetString(1),
                                FlightNumber = reader.GetString(2),
                                PassengerId = reader.GetInt32(3),
                                Class = reader.GetString(4),
                                Status = reader.GetString(5),
                                Luggage = reader.GetDecimal(6),
                                Price = reader.GetDecimal(7)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool CreateTicket(Ticket ticket)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO Ticket (TicketNumber, FlightNumber, PassengerId, Class, Status, Luggage, Price)
                        VALUES (@TicketNumber, @FlightNumber, @PassengerId, @Class, @Status, @Luggage, @Price)";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@TicketNumber", ticket.TicketNumber);
                        cmd.Parameters.AddWithValue("@FlightNumber", ticket.FlightNumber);
                        cmd.Parameters.AddWithValue("@PassengerId", ticket.PassengerId);
                        cmd.Parameters.AddWithValue("@Class", ticket.Class);
                        cmd.Parameters.AddWithValue("@Status", ticket.Status);
                        cmd.Parameters.AddWithValue("@Luggage", ticket.Luggage);
                        cmd.Parameters.AddWithValue("@Price", ticket.Price);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании билета: {ex.Message}");
            }
        }

        public bool UpdateTicket(Ticket ticket)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = @"
                        UPDATE Ticket 
                        SET TicketNumber = @TicketNumber, 
                            FlightNumber = @FlightNumber, 
                            PassengerId = @PassengerId, 
                            Class = @Class, 
                            Status = @Status, 
                            Luggage = @Luggage, 
                            Price = @Price
                        WHERE RecordNumber = @RecordNumber";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@RecordNumber", ticket.RecordNumber);
                        cmd.Parameters.AddWithValue("@TicketNumber", ticket.TicketNumber);
                        cmd.Parameters.AddWithValue("@FlightNumber", ticket.FlightNumber);
                        cmd.Parameters.AddWithValue("@PassengerId", ticket.PassengerId);
                        cmd.Parameters.AddWithValue("@Class", ticket.Class);
                        cmd.Parameters.AddWithValue("@Status", ticket.Status);
                        cmd.Parameters.AddWithValue("@Luggage", ticket.Luggage);
                        cmd.Parameters.AddWithValue("@Price", ticket.Price);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении билета: {ex.Message}");
            }
        }

        public bool DeleteTicket(int recordNumber)
        {
            try
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM Ticket WHERE RecordNumber = @RecordNumber";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@RecordNumber", recordNumber);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении билета: {ex.Message}");
            }
        }

        public List<Ticket> SearchTickets(string searchTerm)
        {
            var tickets = new List<Ticket>();

            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = @"
                    SELECT t.*, p.FullName 
                    FROM Ticket t
                    JOIN Passenger p ON t.PassengerId = p.PassengerId
                    WHERE t.TicketNumber ILIKE @SearchTerm 
                       OR t.FlightNumber ILIKE @SearchTerm
                       OR p.FullName ILIKE @SearchTerm
                    ORDER BY t.RecordNumber";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tickets.Add(new Ticket
                            {
                                RecordNumber = reader.GetInt32(0),
                                TicketNumber = reader.GetString(1),
                                FlightNumber = reader.GetString(2),
                                PassengerId = reader.GetInt32(3),
                                Class = reader.GetString(4),
                                Status = reader.GetString(5),
                                Luggage = reader.GetDecimal(6),
                                Price = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }

            return tickets;
        }
        public int GetSoldTicketsCount(string flightNumber, string ticketClass)
        {
            using (var connection = ConnectionFactory.CreateConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Ticket WHERE FlightNumber = @FlightNumber AND Class = @Class";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FlightNumber", flightNumber);
                    cmd.Parameters.AddWithValue("@Class", ticketClass);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        
    }
}