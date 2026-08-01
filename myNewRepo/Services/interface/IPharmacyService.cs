using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IPharmacyService
    {
        List<PharmacyRecord> GetPharmacyRecords();
        void DispenseMedicine(int pharmacyRecordId, string pharmacistName);
        void DeletePharmacyRecord(int pharmacyRecordId);
    }
}
