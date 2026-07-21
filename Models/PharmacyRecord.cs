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

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public int? TreatmentPlanId { get; set; } // Reference to Doctor's prescription

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
