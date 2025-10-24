using System;

namespace UD_WForms.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public int FlightId { get; set; }
        public int PassengerId { get; set; }
        public string Class { get; set; }
        public string Status { get; set; }
        public bool HasLuggage { get; set; }
        public decimal Price { get; set; }
        public string SeatNumber { get; set; }
        public DateTime BookingDate { get; set; }

        // Навигационные свойства
        public Flight Flight { get; set; }
        public Passenger Passenger { get; set; }
    }
}
