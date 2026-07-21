using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        User ValidateUser(string username, string role);
        List<User> GetDoctors();
        User? GetDoctorById(int doctorId);
        void AssignStaff(string fullName, string username, string role);
        void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email);
        void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email);
        void UpdateUserProfile(string username, string fullName, string contactNumber, string email);
    }
}
