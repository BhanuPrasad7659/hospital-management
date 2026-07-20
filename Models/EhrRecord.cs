using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class EhrRecord
    {
        [Key]
        [Required]
        [StringLength(20)]
        public string EhrId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Diagnosis")]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Doctor Notes")]
        public string DoctorNotes { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Doctor Name")]
        public string DoctorName { get; set; } = string.Empty;
    }
}
