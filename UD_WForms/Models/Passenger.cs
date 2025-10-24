namespace UD_WForms.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public string PassportSeries { get; set; }
        public string PassportIssuedBy { get; set; }
        public DateTime? PassportIssueDate { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}
