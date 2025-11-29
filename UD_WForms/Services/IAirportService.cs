using System.Collections.Generic;
using UD_WForms.Models;

namespace UD_WForms.Services
{
    public interface IAirportService
    {
        List<Airport> GetAllAirports();
        Airport GetAirportById(int airportId);
        Airport GetAirportByIATACode(string iataCode);
        bool CreateAirport(Airport airport);
        bool UpdateAirport(Airport airport);
        bool DeleteAirport(int airportId);
        List<Airport> SearchAirports(string searchTerm);
    }
}