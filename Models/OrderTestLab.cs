using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class OrderTestLab
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LabOrderId { get; set; }

        [Required]
        public int PatientId { get; set; }

        // NEW: Store Patient Name directly in DB
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        public int? DoctorId { get; set; }

        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string? DoctorName { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Test Name")]
        public string TestName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "ORDERED";

        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [StringLength(2000)]
        [Display(Name = "Test Result")]
        public string? Result { get; set; }

        [StringLength(100)]
        [Display(Name = "Technician Name")]
        public string? TechnicianName { get; set; }
    }
}