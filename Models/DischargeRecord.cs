using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CogMediHospitalManagementSystem.Models
{
    public class DischargeRecord
    {
        [Key]
        [Required]
        [StringLength(20)]
        public string DischargeRecordId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AdmissionDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DischargeDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Summary { get; set; } = string.Empty;
    }
}
