using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class PharmacyRecord
    {
        [Key]
        [Required]
        [StringLength(20)]
        public string PharmacyRecordId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [StringLength(20)]
        public string TreatmentPlanId { get; set; } = string.Empty; // Reference to Doctor's prescription

        [ForeignKey(nameof(TreatmentPlanId))]
        public virtual TreatmentPlan? TreatmentPlan { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Medicine Name")]
        public string MedicineName { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "PENDING"; // PENDING, DISPENSED

        [DataType(DataType.DateTime)]
        public DateTime? DispensedDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Pharmacist Name")]
        public string PharmacistName { get; set; } = string.Empty;
    }
}
