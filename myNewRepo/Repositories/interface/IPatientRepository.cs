using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Patient RegisterPatient(Patient patient);
        void UpdatePatient(Patient patient);
        void DeletePatient(int patientId);
        void UpdateStatus(int patientId, string status);
        void AssignDoctorToPatient(int patientId, int doctorId, string doctorName);

        void AssignBedToPatient(int patientId, string ward, string bedNumber);
    }
}
