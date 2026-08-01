using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        User? ValidateUser(string username, string password, string role);
        List<User> GetUsers();
        User? GetUserById(int id);
        User? GetDoctorById(int doctorId);
        void AssignStaff(string fullName, string username, string role, string password);
        void DeleteUser(int id);
        void DeleteUser(string username);
        void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email);
        void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email);
        void UpdateUserProfile(string username, string fullName, string contactNumber, string email);
        void AssignDoctorToPatient(int patientId, int doctorId);
    }
}
