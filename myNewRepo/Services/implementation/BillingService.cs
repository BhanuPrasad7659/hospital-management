using System;
using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;

        public BillingService(IBillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        public List<BillingRecord> GetBillingRecords() => (List<BillingRecord>)_billingRepository.GetAll();

        public BillingRecord? GetBillingForPatient(int patientId) => _billingRepository.GetForPatient(patientId);

        public BillingRecord GenerateBill(int patientId, decimal consultation, decimal lab, decimal medicine, decimal room)
            => _billingRepository.GenerateBill(patientId, consultation, lab, medicine, room);

        public void ProcessPayment(int billingRecordId, string paymentMethod = "UPI", string transactionId = "")
            => _billingRepository.ProcessPayment(billingRecordId, paymentMethod);

        public void DeleteBillingRecord(int billingRecordId)
        {
            var bill = _billingRepository.GetById(billingRecordId);
            if (bill != null)
            {
                _billingRepository.Delete(bill);
                _billingRepository.SaveChanges();
            }
        }

        public decimal GetTotalPaidRevenue() => _billingRepository.GetTotalPaidRevenue();

        public List<decimal> GetMonthlyRevenue(List<DateTime> months) => _billingRepository.GetMonthlyRevenue(months);

        public List<BillingRecord> GetRecentPaidBills(int count) => _billingRepository.GetRecentPaidBills(count);
    }
}
