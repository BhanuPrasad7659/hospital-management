using System;
using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IBillingRepository : IRepository<BillingRecord>
    {
        BillingRecord? GetForPatient(int patientId);
        BillingRecord GenerateBill(int patientId, decimal consultationFee, decimal labCharges, decimal medicineCharges, decimal roomCharges);
        void ProcessPayment(int billingRecordId, string paymentMethod);
        void ClearDischargeStatus(int patientId, string remarks);
        decimal GetTotalPaidRevenue();
        List<decimal> GetMonthlyRevenue(List<DateTime> months);
        List<BillingRecord> GetRecentPaidBills(int count);
    }
}
