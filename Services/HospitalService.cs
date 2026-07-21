using System;
using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories;

namespace CogMediHospitalManagementSystem.Services
{
    public class HospitalService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IEhrRepository _ehrRepository;
        private readonly ILabOrderRepository _labOrderRepository;
        private readonly ITreatmentPlanRepository _treatmentRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IBillingRepository _billingRepository;
        private readonly IDischargeRepository _dischargeRepository;
        private readonly IMedicineStockRepository _medicineStockRepository;

        public HospitalService(
            IUserRepository userRepository,
            IPatientRepository patientRepository,
            IAdmissionRepository admissionRepository,
            IEhrRepository ehrRepository,
            ILabOrderRepository labOrderRepository,
            ITreatmentPlanRepository treatmentRepository,
            IPharmacyRepository pharmacyRepository,
            IBillingRepository billingRepository,
            IDischargeRepository dischargeRepository,
            IMedicineStockRepository medicineStockRepository)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
            _admissionRepository = admissionRepository;
            _ehrRepository = ehrRepository;
            _labOrderRepository = labOrderRepository;
            _treatmentRepository = treatmentRepository;
            _pharmacyRepository = pharmacyRepository;
            _billingRepository = billingRepository;
            _dischargeRepository = dischargeRepository;
            _medicineStockRepository = medicineStockRepository;
        }

        // Dynamic medicine stock dictionary projection via repository
        public Dictionary<string, int> MedicineStock => _medicineStockRepository.GetStockDictionary();

        // --- USER AUTHENTICATION & STAFF MANAGEMENT ---
        public User ValidateUser(string username, string role) => _userRepository.ValidateUser(username, role);

        public List<User> GetUsers() => (List<User>)_userRepository.GetAll();

        public User? GetUserById(int id) => _userRepository.GetById(id);

        public User? GetDoctorById(int doctorId) => _userRepository.GetDoctorById(doctorId);

        public void AssignStaff(string fullName, string username, string role) => _userRepository.AssignStaff(fullName, username, role);

        public void DeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user != null)
            {
                _userRepository.Delete(user);
                _userRepository.SaveChanges();
            }
        }

        public void DeleteUser(string username)
        {
            var user = _userRepository.Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user != null)
            {
                _userRepository.Delete(user);
                _userRepository.SaveChanges();
            }
        }

        public void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email)
            => _userRepository.UpdateDoctorProfile(doctorId, specialty, biography, contactNumber, email);

        public void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email)
            => _userRepository.UpdateDoctorProfile(username, specialty, biography, contactNumber, email);

        public void UpdateUserProfile(string username, string fullName, string contactNumber, string email)
            => _userRepository.UpdateUserProfile(username, fullName, contactNumber, email);

        public void AssignDoctorToPatient(int patientId, int doctorId)
        {
            var doctor = _userRepository.GetDoctorById(doctorId);
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            _patientRepository.AssignDoctorToPatient(patientId, doctorId, doctorName);
            _admissionRepository.AssignDoctorToAdmission(patientId, doctorId, doctorName);
        }

        // --- PATIENTS ---
        public List<Patient> GetPatients() => (List<Patient>)_patientRepository.GetAll();

        public Patient? GetPatient(int patientId) => _patientRepository.GetById(patientId);

        public Patient RegisterPatient(Patient patient) => _patientRepository.RegisterPatient(patient);

        public void UpdatePatient(Patient patient) => _patientRepository.UpdatePatient(patient);

        public void DeletePatient(int patientId) => _patientRepository.DeletePatient(patientId);

        // --- ADMISSIONS ---
        public List<Admission> GetAdmissions() => (List<Admission>)_admissionRepository.GetAll();

        public Admission? GetAdmission(int admissionId) => _admissionRepository.GetById(admissionId);

        public Admission AdmitPatient(int patientId, string ward, string bedNumber, int doctorId)
        {
            var doctor = _userRepository.GetDoctorById(doctorId);
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            _patientRepository.UpdateStatus(patientId, "ADMITTED");
            _patientRepository.AssignDoctorToPatient(patientId, doctorId, doctorName);

            return _admissionRepository.AdmitPatient(patientId, ward, bedNumber, doctorId, doctorName);
        }

        public void DeleteAdmission(int admissionId) => _admissionRepository.DeleteAdmission(admissionId);

        public void UpdateAdmission(Admission admission) => _admissionRepository.UpdateAdmission(admission);

        public WardAnalyticsData GetWardAdmissionsData() => _admissionRepository.GetWardAdmissionsData();

        public List<WardSummaryItem> GetWardSummaries() => _admissionRepository.GetWardSummaries();

        // --- EHR RECORDS ---
        public List<EhrRecord> GetEhrRecords() => (List<EhrRecord>)_ehrRepository.GetAll();

        public List<EhrRecord> GetEhrForPatient(int patientId) => _ehrRepository.GetEhrForPatient(patientId);

        public EhrRecord CreateEhr(EhrRecord record) => _ehrRepository.CreateEhr(record);

        // --- LAB ORDERS ---
        public List<LabOrder> GetLabOrders() => (List<LabOrder>)_labOrderRepository.GetAll();

        public List<LabOrder> GetLabOrdersForPatient(int patientId) => _labOrderRepository.GetLabOrdersForPatient(patientId);

        public LabOrder CreateLabOrder(LabOrder order) => _labOrderRepository.CreateLabOrder(order);

        public void UpdateLabOrderStatus(int orderId, string status, string result, string technicianName)
            => _labOrderRepository.UpdateLabOrderStatus(orderId, status, result, technicianName);

        public void DeleteLabOrder(int orderId) => _labOrderRepository.DeleteLabOrder(orderId);

        public LabAnalyticsData GetLabTestVolumesData() => _labOrderRepository.GetLabTestVolumesData();

        // --- TREATMENT PLANS ---
        public List<TreatmentPlan> GetTreatments() => (List<TreatmentPlan>)_treatmentRepository.GetAll();

        public List<TreatmentPlan> GetTreatmentsForPatient(int patientId) => _treatmentRepository.GetTreatmentsForPatient(patientId);

        public TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan) => _treatmentRepository.CreateTreatmentPlan(plan);

        public void DeleteTreatmentPlan(int planId) => _treatmentRepository.DeleteTreatmentPlan(planId);

        public void UpdateTreatmentPlan(TreatmentPlan plan) => _treatmentRepository.UpdateTreatmentPlan(plan);

        // --- PHARMACY ---
        public List<PharmacyRecord> GetPharmacyRecords() => (List<PharmacyRecord>)_pharmacyRepository.GetAll();

        public void DispenseMedicine(int pharmacyRecordId, string pharmacistName)
            => _pharmacyRepository.DispenseMedicine(pharmacyRecordId, pharmacistName);

        public void UpdateMedicineStock(string medicineName, int amount)
            => _medicineStockRepository.UpdateStock(medicineName, amount);

        public void DeletePharmacyRecord(int pharmacyRecordId) => _pharmacyRepository.DeleteRecord(pharmacyRecordId);

        // --- BILLING & DISCHARGE ---
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

        public void DischargePatient(int patientId, string remarks)
            => _billingRepository.ClearDischargeStatus(patientId, remarks);

        public List<DischargeRecord> GetDischargeRecords() => _dischargeRepository.GetDischargeHistory();

        public decimal GetTotalPaidRevenue() => _billingRepository.GetTotalPaidRevenue();

        public List<decimal> GetMonthlyRevenue(List<DateTime> months) => _billingRepository.GetMonthlyRevenue(months);

        public List<BillingRecord> GetRecentPaidBills(int count) => _billingRepository.GetRecentPaidBills(count);
    }
}
