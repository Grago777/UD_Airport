using System;

namespace UD_WForms.Models
{
    public class Flight
    {
        public string FlightNumber { get; set; }
        public string FlightType { get; set; }
        public string Aircraft { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public TimeSpan FlightTime { get; set; }
        public string Status { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public int EconomySeats { get; set; }
        public int BusinessSeats { get; set; }
        public string Airline { get; set; }
    }
}