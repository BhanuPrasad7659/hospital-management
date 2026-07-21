using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CogMediHospitalManagementSystem.Enums;

namespace CogMediHospitalManagementSystem.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Patient ID")]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 150)]
        public int Age { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty; // Male, Female, Other

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "REGISTERED"; // REGISTERED, ADMITTED, DISCHARGED

        [DataType(DataType.DateTime)]
        [Display(Name = "Registration Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Doctor Assignment Tracking using int DoctorId
        public int? AssignedDoctorId { get; set; }
        
        [ForeignKey(nameof(AssignedDoctorId))]
        public virtual User? AssignedDoctor { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned Doctor")]
        public string AssignedDoctorName { get; set; } = string.Empty;

        // Enum Helper Properties
        [NotMapped]
        public PatientStatus StatusEnum
        {
            get => Enum.TryParse<PatientStatus>(Status, true, out var result) ? result : PatientStatus.Unknown;
            set => Status = value.ToString().ToUpper();
        }

        [NotMapped]
        public GenderType GenderEnum
        {
            get => Enum.TryParse<GenderType>(Gender, true, out var result) ? result : GenderType.Unknown;
            set => Gender = value.ToString();
        }
    }
}
