using System;

namespace UD_WForms.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public string FlightType { get; set; }
        public string Aircraft { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public string Status { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int SeatsEconomy { get; set; }
        public int SeatsBusiness { get; set; }
        public string Airline { get; set; }

        // Навигационные свойства
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
    }
}

