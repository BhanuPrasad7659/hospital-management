using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class Patient
    {
        public string PatientId { get; set; } = string.Empty; // e.g., "P000248"
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty; // Male, Female, Other
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "REGISTERED"; // REGISTERED, ADMITTED, DISCHARGED
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
