using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class PharmacyRecord
    {
        public string PharmacyRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string TreatmentPlanId { get; set; } = string.Empty; // Reference to Doctor's prescription
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, DISPENSED
        public DateTime? DispensedDate { get; set; }
        public string PharmacistName { get; set; } = string.Empty;
    }
}
