using System;
using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IBillingService
    {
        List<BillingRecord> GetBillingRecords();
        BillingRecord? GetBillingForPatient(int patientId);
        BillingRecord GenerateBill(int patientId, decimal consultation, decimal lab, decimal medicine, decimal room);
        void ProcessPayment(int billingRecordId, string paymentMethod = "UPI", string transactionId = "");
        void DeleteBillingRecord(int billingRecordId);
        decimal GetTotalPaidRevenue();
        List<decimal> GetMonthlyRevenue(List<DateTime> months);
        List<BillingRecord> GetRecentPaidBills(int count);
    }
}
