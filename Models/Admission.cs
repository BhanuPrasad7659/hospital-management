using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class Admission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdmissionId { get; set; }

        [Required]
        public int PatientId { get; set; }

        // NEW: Store Patient Name directly in DB
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        // NEW: Store Age directly in DB and enforce Age > 0
        [Required]
        [Range(1, 150, ErrorMessage = "Age must be greater than 0.")]
        public int Age { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? DischargeDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ward")]
        public string Ward { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Bed Number")]
        public string BedNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "ADMITTED";

        public int? AssignedDoctorId { get; set; }

        [ForeignKey(nameof(AssignedDoctorId))]
        public virtual User? AssignedDoctor { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned Doctor")]
        public string AssignedDoctorName { get; set; } = string.Empty;
    }
}