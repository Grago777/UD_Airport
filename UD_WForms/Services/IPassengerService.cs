using System.Collections.Generic;
using UD_WForms.Models;

namespace UD_WForms.Services
{
    public interface IPassengerService
    {
        List<Passenger> GetAllPassengers();
        Passenger GetPassengerById(int passengerId);
        bool CreatePassenger(Passenger passenger);
        bool UpdatePassenger(Passenger passenger);
        bool DeletePassenger(int passengerId);
        List<Passenger> SearchPassengers(string searchTerm);
    }
}