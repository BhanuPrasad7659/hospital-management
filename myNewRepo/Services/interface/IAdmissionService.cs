using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IAdmissionService
    {
        List<Admission> GetAdmissions();
        Admission? GetAdmission(int admissionId);
        Admission AdmitPatient(int patientId, string ward, string bedNumber, int doctorId);
        void DeleteAdmission(int admissionId);
        void UpdateAdmission(Admission admission);
        WardAnalyticsData GetWardAdmissionsData();
        List<WardSummaryItem> GetWardSummaries();
    }
}
