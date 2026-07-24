using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class EhrService : IEhrService
    {
        private readonly IEhrRepository _ehrRepository;

        public EhrService(IEhrRepository ehrRepository)
        {
            _ehrRepository = ehrRepository;
        }

        public List<EhrRecord> GetEhrRecords() => (List<EhrRecord>)_ehrRepository.GetAll();

        public List<EhrRecord> GetEhrForPatient(int patientId) => _ehrRepository.GetEhrForPatient(patientId);

        public EhrRecord CreateEhr(EhrRecord record) => _ehrRepository.CreateEhr(record);
    }
}
