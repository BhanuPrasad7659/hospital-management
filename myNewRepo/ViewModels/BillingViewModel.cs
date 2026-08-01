using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.ViewModels
{
    public class BillingViewModel
    {
        public int BillingRecordId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public decimal LabCharges { get; set; }
        public decimal MedicineCharges { get; set; }
        public decimal RoomCharges { get; set; }
        public decimal TotalAmount => ConsultationFee + LabCharges + MedicineCharges + RoomCharges;
        public string Status { get; set; } = "PENDING"; 

        public bool IsDischarged { get; set; }
        public string DischargeRemarks { get; set; } = string.Empty;
        
        public Patient? Patient { get; set; }
    }
}
