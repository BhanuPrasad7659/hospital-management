using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class AdmissionService : IAdmissionService
    {
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;

        public AdmissionService(
            IAdmissionRepository admissionRepository,
            IPatientRepository patientRepository,
            IUserRepository userRepository)
        {
            _admissionRepository = admissionRepository;
            _patientRepository = patientRepository;
            _userRepository = userRepository;
        }

        public List<Admission> GetAdmissions() => (List<Admission>)_admissionRepository.GetAll();

        public Admission? GetAdmission(int admissionId) => _admissionRepository.GetById(admissionId);

        public Admission AdmitPatient(int patientId, string ward, string bedNumber, int doctorId)
        {
            var doctor = _userRepository.GetDoctorById(doctorId);
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            _patientRepository.UpdateStatus(patientId, "ADMITTED");
            _patientRepository.AssignDoctorToPatient(patientId, doctorId, doctorName);
            _patientRepository.AssignBedToPatient(patientId, ward, bedNumber);

            return _admissionRepository.AdmitPatient(patientId, ward, bedNumber, doctorId, doctorName);
        }

        public void DeleteAdmission(int admissionId) => _admissionRepository.DeleteAdmission(admissionId);

        public void UpdateAdmission(Admission admission) => _admissionRepository.UpdateAdmission(admission);

        public WardAnalyticsData GetWardAdmissionsData() => _admissionRepository.GetWardAdmissionsData();

        public List<WardSummaryItem> GetWardSummaries() => _admissionRepository.GetWardSummaries();
    }
}
