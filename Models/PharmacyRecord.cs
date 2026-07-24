using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class PharmacyRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PharmacyRecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        // NEW: Store Patient Name directly in DB
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public int? TreatmentPlanId { get; set; }

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
        public string Status { get; set; } = "PENDING";

        [DataType(DataType.DateTime)]
        public DateTime? DispensedDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Pharmacist Name")]
        public string PharmacistName { get; set; } = string.Empty;
    }
}