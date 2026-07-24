using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAdmissionRepository _admissionRepository;

        public UserService(
            IUserRepository userRepository,
            IPatientRepository patientRepository,
            IAdmissionRepository admissionRepository)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
            _admissionRepository = admissionRepository;
        }

        public User? ValidateUser(string username, string password, string role) => _userRepository.ValidateUser(username, password, role);

        public List<User> GetUsers() => (List<User>)_userRepository.GetAll();

        public User? GetUserById(int id) => _userRepository.GetById(id);

        public User? GetDoctorById(int doctorId) => _userRepository.GetDoctorById(doctorId);

        public void AssignStaff(string fullName, string username, string role, string password) => _userRepository.AssignStaff(fullName, username, role, password);

        public void DeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user != null)
            {
                _userRepository.Delete(user);
                _userRepository.SaveChanges();
            }
        }

        public void DeleteUser(string username)
        {
            var user = _userRepository.Get(u => u.Username.ToLower() == username.ToLower());
            if (user != null)
            {
                _userRepository.Delete(user);
                _userRepository.SaveChanges();
            }
        }

        public void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email)
            => _userRepository.UpdateDoctorProfile(doctorId, specialty, biography, contactNumber, email);

        public void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email)
            => _userRepository.UpdateDoctorProfile(username, specialty, biography, contactNumber, email);

        public void UpdateUserProfile(string username, string fullName, string contactNumber, string email)
            => _userRepository.UpdateUserProfile(username, fullName, contactNumber, email);

        public void AssignDoctorToPatient(int patientId, int doctorId)
        {
            var doctor = _userRepository.GetDoctorById(doctorId);
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            _patientRepository.AssignDoctorToPatient(patientId, doctorId, doctorName);
            _admissionRepository.AssignDoctorToAdmission(patientId, doctorId, doctorName);
        }
    }
}
