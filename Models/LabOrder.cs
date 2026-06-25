using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class LabOrder
    {
        public string LabOrderId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty; // CBC Test, X-Ray, MRI, etc.
        public string Status { get; set; } = "ORDERED"; // ORDERED, IN_PROGRESS, COMPLETED
        public string Result { get; set; } = string.Empty; // Results text, e.g., "Hemoglobin 14.2 g/dL (Normal)"
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string DoctorName { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
    }
}
