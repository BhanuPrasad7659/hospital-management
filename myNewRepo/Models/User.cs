using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CogMediHospitalManagementSystem.Enums;

namespace CogMediHospitalManagementSystem.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = "password";

        [StringLength(20)]
        [Display(Name = "Doctor ID")]
        public string? DoctorUniqueId { get; set; }

        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(500)]
        public string Biography { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        public UserRole RoleEnum
        {
            get => Enum.TryParse<UserRole>(Role.Replace(" ", ""), true, out var result) ? result : UserRole.Unknown;

            set => Role = value == UserRole.BillingDischarge ? "billing discharge" : value.ToString().ToLower();
        }
    }
}
