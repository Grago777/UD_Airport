namespace UD_WForms.Models
{
    public class Ticket
    {
        public int RecordNumber { get; set; }
        public string TicketNumber { get; set; }
        public string FlightNumber { get; set; }
        public int PassengerId { get; set; }
        public string Class { get; set; }
        public string Status { get; set; }
        public decimal Luggage { get; set; }
        public decimal Price { get; set; }
    }
}