using System.ComponentModel.DataAnnotations;

namespace CogMediHospitalManagementSystem.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserCreateDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(admin|receptionist|doctor|laboratory|pharmacist|billing discharge)$", ErrorMessage = "Invalid role value. Must be 'admin', 'receptionist', 'doctor', 'laboratory', 'pharmacist', or 'billing discharge'.")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        public string Specialty { get; set; } = string.Empty;
        
        public string Biography { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Password { get; set; }
    }

    public class UserUpdateDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [RegularExpression("^(admin|receptionist|doctor|laboratory|pharmacist|billing discharge)$", ErrorMessage = "Invalid role value.")]
        public string Role { get; set; } = string.Empty;

        public string Specialty { get; set; } = string.Empty;
        
        public string Biography { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Password { get; set; }
    }
}
