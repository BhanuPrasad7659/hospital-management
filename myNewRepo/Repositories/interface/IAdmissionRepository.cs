using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public class WardAnalyticsData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();
    }

    public class WardSummaryItem
    {
        public string WardName { get; set; } = string.Empty;
        public int ActiveAdmissions { get; set; }
        public int TotalDischarged { get; set; }
        public int TotalAdmissions { get; set; }
    }

    public interface IAdmissionRepository : IRepository<Admission>
    {
        Admission AdmitPatient(int patientId, string ward, string bedNumber, int doctorId, string doctorName);
        void UpdateAdmission(Admission admission);
        void DeleteAdmission(int admissionId);
        void DischargeAdmission(int patientId);
        void AssignDoctorToAdmission(int patientId, int doctorId, string doctorName);
        WardAnalyticsData GetWardAdmissionsData();
        List<WardSummaryItem> GetWardSummaries();
    }
}
