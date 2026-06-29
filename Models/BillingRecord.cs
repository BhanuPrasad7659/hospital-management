using System;

namespace CogMediHospitalManagementSystem.Models
{
    public class BillingRecord
    {
        public string BillingRecordId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public decimal LabCharges { get; set; }
        public decimal MedicineCharges { get; set; }
        public decimal RoomCharges { get; set; }
        public decimal TotalAmount => ConsultationFee + LabCharges + MedicineCharges + RoomCharges;
        public string Status { get; set; } = "PENDING"; // PENDING, PAID
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? PaymentDate { get; set; }
        
        // Merged Discharge properties (RBAC)
        public bool IsDischarged { get; set; } = false;
        public DateTime? DischargeDate { get; set; }
        public string DischargeRemarks { get; set; } = string.Empty;
    }
}
