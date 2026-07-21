using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IPharmacyRepository : IRepository<PharmacyRecord>
    {
        List<PharmacyRecord> GetRecordsForPatient(int patientId);
        PharmacyRecord CreateRecord(PharmacyRecord record);
        void DispenseMedicine(int recordId, string pharmacistName);
        void DeleteRecord(int recordId);
    }
}
