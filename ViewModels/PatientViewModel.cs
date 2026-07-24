using System.ComponentModel.DataAnnotations;

namespace CogMediHospitalManagementSystem.ViewModels
{
    public class PatientViewModel
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Age is required.")]
        [Range(1, 150, ErrorMessage = "Age must be greater than 0.")]
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "REGISTERED";
        
        // Ward assignment for Admission
        public string Ward { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public int? AssignedDoctorId { get; set; }
    }
}
