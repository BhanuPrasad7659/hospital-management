using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class TreatmentPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TreatmentPlanId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public int? DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual User? Doctor { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Diagnosis")]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Treatment Description")]
        public string TreatmentDescription { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Medication")]
        public string Medication { get; set; } = string.Empty; // e.g., "Paracetamol 500mg"

        [StringLength(100)]
        [Display(Name = "Duration")]
        public string Duration { get; set; } = string.Empty; // e.g., "5 Days"

        [StringLength(500)]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; } = string.Empty; // e.g., "Take twice a day after meals"

        [DataType(DataType.DateTime)]
        [Display(Name = "Prescribed Date")]
        public DateTime PrescribedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Order Test")]
        public string? OrderTest { get; set; } // Optional: If filled, triggers lab order creation
    }
}
