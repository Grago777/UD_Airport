using System.Collections.Generic;
using UD_WForms.Models;

namespace UD_WForms.Services
{
    public interface IFlightService
    {
        List<Flight> GetAllFlights();
        Flight GetFlightByNumber(string flightNumber);
        bool CreateFlight(Flight flight);
        bool UpdateFlight(Flight flight);
        bool DeleteFlight(string flightNumber);
        List<Flight> SearchFlights(string searchTerm);
        List<Flight> GetFlightsByAirport(int airportId);
    }
}