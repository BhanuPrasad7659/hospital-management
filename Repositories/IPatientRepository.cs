using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Patient RegisterPatient(Patient patient);
        void UpdatePatient(Patient patient);
        void DeletePatient(int patientId);
        void AssignDoctorToPatient(int patientId, int doctorId, string doctorName);
        void UpdateStatus(int patientId, string status);
    }
}
