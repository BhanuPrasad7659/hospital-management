using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class EhrRecord
    {
        public string EhrId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; } = DateTime.Now;
        public string DoctorName { get; set; } = string.Empty;
    }
}
