using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class BillingRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillingRecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        [Display(Name = "Lab Charges")]
        public decimal LabCharges { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        [Display(Name = "Medicine Charges")]
        public decimal MedicineCharges { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        [Display(Name = "Room Charges")]
        public decimal RoomCharges { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount => ConsultationFee + LabCharges + MedicineCharges + RoomCharges;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "PENDING"; // PENDING, PAID

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? PaymentDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty; // e.g. UPI (PhonePe), UPI (GPay), Cash, Card

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty; // e.g. TXN-98420194
        
        // Merged Discharge properties (RBAC)
        public bool IsDischarged { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime? DischargeDate { get; set; }

        [StringLength(500)]
        public string DischargeRemarks { get; set; } = string.Empty;
    }
}
