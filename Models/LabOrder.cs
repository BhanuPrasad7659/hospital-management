using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class LabOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabOrderId { get; set; }
        
        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public int? DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual User? Doctor { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Test Name")]
        public string TestName { get; set; } = string.Empty; // CBC Test, X-Ray, MRI, etc.

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "ORDERED"; // ORDERED, IN_PROGRESS, COMPLETED

        [StringLength(1000)]
        [Display(Name = "Lab Result")]
        public string Result { get; set; } = string.Empty; // Results text, e.g., "Hemoglobin 14.2 g/dL (Normal)"

        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Technician Name")]
        public string TechnicianName { get; set; } = string.Empty;
    }
}
