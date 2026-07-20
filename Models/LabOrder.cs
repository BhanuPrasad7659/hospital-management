using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class LabOrder
    {
        [Key]
        [Required]
        [StringLength(20)]
        public string LabOrderId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

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
