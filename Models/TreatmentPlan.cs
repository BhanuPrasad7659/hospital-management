using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class TreatmentPlan
    {
        public string TreatmentPlanId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentDescription { get; set; } = string.Empty;
        public string Medication { get; set; } = string.Empty; // e.g., "Paracetamol 500mg"
        public string Duration { get; set; } = string.Empty; // e.g., "5 Days"
        public string Instructions { get; set; } = string.Empty; // e.g., "Take twice a day after meals"
        public DateTime PrescribedDate { get; set; } = DateTime.Now;
        public string DoctorName { get; set; } = string.Empty;
    }
}
