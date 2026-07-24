using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? ValidateUser(string username, string password, string role);
        List<User> GetDoctors();
        User? GetDoctorById(int doctorId);
        void AssignStaff(string fullName, string username, string role, string password);
        void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email);
        void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email);
        void UpdateUserProfile(string username, string fullName, string contactNumber, string email);
    }
}
