using System.Collections.Generic;
using UD_WForms.Models;

namespace UD_WForms.Services
{
    public interface ITicketService
    {
        List<Ticket> GetAllTickets();
        Ticket GetTicketById(int recordNumber);
        bool CreateTicket(Ticket ticket);
        bool UpdateTicket(Ticket ticket);
        bool DeleteTicket(int recordNumber);
        List<Ticket> SearchTickets(string searchTerm);
    }
}