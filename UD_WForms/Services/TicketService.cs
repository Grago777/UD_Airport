using Npgsql;
using UD_WForms.Models;
using UD_WForms.Models.Database;
using System.Collections.Generic;
using System;

namespace UD_WForms.Services
{
    public class TicketService
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
                           p.FullName, f.DepartureDate, f.ArrivalDate
                    FROM Ticket t
                    JOIN Passenger p ON t.PassengerId = p.PassengerId
                    JOIN Flight f ON t.FlightNumber = f.FlightNumber
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}