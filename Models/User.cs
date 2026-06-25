namespace CogMediHospitalManagementSystem.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // admin, receptionist, doctor, laboratory, pharmacist, billing discharge
        public string FullName { get; set; } = string.Empty;
    }
}
