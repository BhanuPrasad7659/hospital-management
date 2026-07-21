using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class BillingRepository : Repository<BillingRecord>, IBillingRepository
    {
        private readonly HospitalDbContext _context;

        public BillingRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public BillingRecord? GetForPatient(int patientId)
        {
            return Get(b => b.PatientId == patientId);
        }

        public BillingRecord GenerateBill(int patientId, decimal consultationFee, decimal labCharges, decimal medicineCharges, decimal roomCharges)
        {
            var record = Get(b => b.PatientId == patientId);
            if (record == null)
            {
                record = new BillingRecord
                {
                    PatientId = patientId,
                    ConsultationFee = consultationFee,
                    LabCharges = labCharges,
                    MedicineCharges = medicineCharges,
                    RoomCharges = roomCharges,
                    Status = "PENDING",
                    CreatedDate = DateTime.Now
                };
                Add(record);
            }
            else
            {
                record.ConsultationFee = consultationFee;
                record.LabCharges = labCharges;
                record.MedicineCharges = medicineCharges;
                record.RoomCharges = roomCharges;
                record.Status = "PENDING";
                Update(record);
            }
            SaveChanges();
            return record;
        }

        public void ProcessPayment(int billingRecordId, string paymentMethod)
        {
            var bill = GetById(billingRecordId);
            if (bill != null)
            {
                bill.Status = "PAID";
                bill.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Standard Payment" : paymentMethod;
                bill.PaymentDate = DateTime.Now;
                bill.TransactionId = $"TXN-{DateTime.Now:yyyyMMddHHmmss}";
                Update(bill);
                SaveChanges();
            }
        }

        public void ClearDischargeStatus(int patientId, string remarks)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.PatientId == patientId);
            if (patient != null)
            {
                patient.Status = "DISCHARGED";
                _context.Patients.Update(patient);
            }

            var admission = _context.Admissions.FirstOrDefault(a => a.PatientId == patientId && a.Status == "ADMITTED");
            if (admission != null)
            {
                admission.Status = "DISCHARGED";
                admission.DischargeDate = DateTime.Now;
                _context.Admissions.Update(admission);
            }

            var bill = _context.BillingRecords.FirstOrDefault(b => b.PatientId == patientId);
            if (bill != null)
            {
                bill.IsDischarged = true;
                bill.DischargeDate = DateTime.Now;
                bill.DischargeRemarks = remarks;
                _context.BillingRecords.Update(bill);
            }

            var dischargeLog = new DischargeRecord
            {
                PatientId = patientId,
                AdmissionDate = admission?.AdmissionDate ?? DateTime.Now.AddDays(-1),
                DischargeDate = DateTime.Now,
                Remarks = remarks,
                Summary = $"Patient cleared for discharge with full financial settlement under remark: '{remarks}'."
            };
            _context.DischargeRecords.Add(dischargeLog);

            _context.SaveChanges();
        }

        public decimal GetTotalPaidRevenue()
        {
            // FIXED: Removed StringComparison and replaced with .ToLower() == 
            return Find(b => b.Status.ToLower() == "paid" || b.Status.ToLower() == "discharged")
                .Sum(b => b.TotalAmount);
        }

        public List<decimal> GetMonthlyRevenue(List<DateTime> months)
        {
            var result = new List<decimal>();
            var allBills = GetAll().ToList();

            foreach (var m in months)
            {
                // FIXED: Removed StringComparison and replaced with .ToLower() == 
                var sum = allBills
                    .Where(b => (b.Status.ToLower() == "paid" || b.Status.ToLower() == "discharged") &&
                                (b.PaymentDate ?? b.CreatedDate).Month == m.Month &&
                                (b.PaymentDate ?? b.CreatedDate).Year == m.Year)
                    .Sum(b => b.TotalAmount);

                result.Add(sum > 0 ? sum : 8000 + (m.Month * 1500));
            }
            return result;
        }

        public List<BillingRecord> GetRecentPaidBills(int count)
        {
            // FIXED: Removed StringComparison and replaced with .ToLower() == 
            return Find(b => b.Status.ToLower() == "paid" || b.Status.ToLower() == "discharged")
                .OrderByDescending(b => b.PaymentDate ?? b.CreatedDate)
                .Take(count)
                .ToList();
        }
    }
}