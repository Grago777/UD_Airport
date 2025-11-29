using System;

namespace UD_WForms.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PassportData { get; set; }
    }
}