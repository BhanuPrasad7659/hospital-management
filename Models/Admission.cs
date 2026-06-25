using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class Admission
    {
        public string AdmissionId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateTime? DischargeDate { get; set; }
        public string Ward { get; set; } = string.Empty; // General, ICU, Pediatrics, etc.
        public string BedNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "ADMITTED"; // ADMITTED, DISCHARGED
    }
}
