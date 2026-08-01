using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public List<Patient> GetPatients() => (List<Patient>)_patientRepository.GetAll();

        public Patient? GetPatient(int patientId) => _patientRepository.GetById(patientId);

        public Patient RegisterPatient(Patient patient) => _patientRepository.RegisterPatient(patient);

        public void UpdatePatient(Patient patient) => _patientRepository.UpdatePatient(patient);

        public void DeletePatient(int patientId) => _patientRepository.DeletePatient(patientId);
    }
}
