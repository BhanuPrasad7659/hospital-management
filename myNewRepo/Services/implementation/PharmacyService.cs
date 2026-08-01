using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IPharmacyRepository _pharmacyRepository;

        public PharmacyService(IPharmacyRepository pharmacyRepository)
        {
            _pharmacyRepository = pharmacyRepository;
        }

        public List<PharmacyRecord> GetPharmacyRecords() => (List<PharmacyRecord>)_pharmacyRepository.GetAll();

        public void DispenseMedicine(int pharmacyRecordId, string pharmacistName)
            => _pharmacyRepository.DispenseMedicine(pharmacyRecordId, pharmacistName);

        public void DeletePharmacyRecord(int pharmacyRecordId) => _pharmacyRepository.DeleteRecord(pharmacyRecordId);
    }
}
