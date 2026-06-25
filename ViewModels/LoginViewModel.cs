namespace CogMediHospitalManagementSystem.ViewModels
{
    public class LoginViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Receptionist, etc.
        public bool RememberMe { get; set; }
    }
}
