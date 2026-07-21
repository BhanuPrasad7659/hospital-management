using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class EhrRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EhrId { get; set; }
        
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
