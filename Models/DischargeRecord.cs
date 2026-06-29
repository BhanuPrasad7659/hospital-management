using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class DischargeRecord
    {
        public string DischargeRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public DateTime DischargeDate { get; set; } = DateTime.Now;
        public string Remarks { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
