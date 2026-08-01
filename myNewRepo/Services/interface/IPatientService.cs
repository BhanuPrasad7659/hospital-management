using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IPatientService
    {
        List<Patient> GetPatients();
        Patient? GetPatient(int patientId);
        Patient RegisterPatient(Patient patient);
        void UpdatePatient(Patient patient);
        void DeletePatient(int patientId);
    }
}
