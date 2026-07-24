using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class DischargeService : IDischargeService
    {
        private readonly IDischargeRepository _dischargeRepository;
        private readonly IBillingRepository _billingRepository;

        public DischargeService(
            IDischargeRepository dischargeRepository,
            IBillingRepository billingRepository)
        {
            _dischargeRepository = dischargeRepository;
            _billingRepository = billingRepository;
        }

        public void DischargePatient(int patientId, string remarks)
            => _billingRepository.ClearDischargeStatus(patientId, remarks);

        public List<DischargeRecord> GetDischargeRecords() => _dischargeRepository.GetDischargeHistory();
    }
}
