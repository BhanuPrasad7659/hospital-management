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

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? DischargeDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ward")]
        public string Ward { get; set; } = string.Empty; // General, ICU, Pediatrics, etc.

        [Required]
        [StringLength(20)]
        [Display(Name = "Bed Number")]
        public string BedNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "ADMITTED"; // ADMITTED, DISCHARGED

        // Doctor Assignment Tracking using int DoctorId
        public int? AssignedDoctorId { get; set; }

        [ForeignKey(nameof(AssignedDoctorId))]
        public virtual User? AssignedDoctor { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned Doctor")]
        public string AssignedDoctorName { get; set; } = string.Empty;
    }
}
