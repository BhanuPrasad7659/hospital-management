using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories;

namespace CogMediHospitalManagementSystem.Services
{
    public class HospitalService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Patient> _patientRepository;
        private readonly IRepository<Admission> _admissionRepository;
        private readonly IRepository<EhrRecord> _ehrRepository;
        private readonly IRepository<LabOrder> _labOrderRepository;
        private readonly IRepository<TreatmentPlan> _treatmentRepository;
        private readonly IRepository<PharmacyRecord> _pharmacyRepository;
        private readonly IRepository<BillingRecord> _billingRepository;
        private readonly IRepository<DischargeRecord> _dischargeRepository;
        private readonly IRepository<MedicineStock> _medicineStockRepository;

        public HospitalService(
            IRepository<User> userRepository,
            IRepository<Patient> patientRepository,
            IRepository<Admission> admissionRepository,
            IRepository<EhrRecord> ehrRepository,
            IRepository<LabOrder> labOrderRepository,
            IRepository<TreatmentPlan> treatmentRepository,
            IRepository<PharmacyRecord> pharmacyRepository,
            IRepository<BillingRecord> billingRepository,
            IRepository<DischargeRecord> dischargeRepository,
            IRepository<MedicineStock> medicineStockRepository)
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

        // Projecting the DB medicine stocks dynamically as a Dictionary to match original property signature
        public Dictionary<string, int> MedicineStock => 
            _medicineStockRepository.GetAll().ToDictionary(m => m.MedicineName, m => m.Quantity);

        // --- USER AUTHENTICATION ---
        public User ValidateUser(string username, string role)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = role;
            }

            var user = _userRepository.Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                string defaultName = username;
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase)) defaultName = "System Administrator";
                else if (username.Equals("receptionist", StringComparison.OrdinalIgnoreCase)) defaultName = "Receptionist Staff";
                else if (username.Equals("doctor", StringComparison.OrdinalIgnoreCase)) defaultName = "Dr. Sarah Jenkins";
                else if (username.Equals("laboratory", StringComparison.OrdinalIgnoreCase)) defaultName = "Diagnostic Lab Technician";
                else if (username.Equals("pharmiacist", StringComparison.OrdinalIgnoreCase)) defaultName = "Chief Pharmacist";
                else if (username.Equals("billing discharge", StringComparison.OrdinalIgnoreCase)) defaultName = "Billing Officer";

                user = new User { Username = username, Role = role, FullName = defaultName };
                _userRepository.Add(user);
                _userRepository.SaveChanges();
            }
            return user;
        }

        public List<User> GetUsers()
        {
            return _userRepository.GetAll().ToList();
        }

        public void AssignStaff(string fullName, string username, string role)
        {
            var existing = _userRepository.Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Role = role;
                _userRepository.Update(existing);
            }
            else
            {
                _userRepository.Add(new User { FullName = fullName, Username = username, Role = role });
            }
            _userRepository.SaveChanges();
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

        // --- PATIENTS ---
        public List<Patient> GetPatients()
        {
            return _patientRepository.GetAll().ToList();
        }

        public Patient? GetPatient(string patientId)
        {
            return _patientRepository.Get(p => p.PatientId == patientId);
        }

        public Patient RegisterPatient(Patient patient)
        {
            int nextNum = _patientRepository.GetAll().Count() + 101;
            patient.PatientId = $"P{nextNum}";
            patient.Status = "REGISTERED";
            patient.CreatedDate = DateTime.Now;
            _patientRepository.Add(patient);
            _patientRepository.SaveChanges();
            return patient;
        }

        public void UpdatePatient(Patient patient)
        {
            var existing = _patientRepository.Get(p => p.PatientId == patient.PatientId);
            if (existing != null)
            {
                existing.Name = patient.Name;
                existing.Age = patient.Age;
                existing.Gender = patient.Gender;
                existing.Address = patient.Address;
                existing.ContactNumber = patient.ContactNumber;
                existing.Status = patient.Status;
                _patientRepository.Update(existing);
                _patientRepository.SaveChanges();
            }
        }

        public void DeletePatient(string patientId)
        {
            var patient = _patientRepository.Get(p => p.PatientId == patientId);
            if (patient != null)
            {
                _patientRepository.Delete(patient);
                _patientRepository.SaveChanges();
            }
        }

        // --- ADMISSIONS ---
        public List<Admission> GetAdmissions()
        {
            return _admissionRepository.GetAll().ToList();
        }

        public Admission AdmitPatient(string patientId, string ward, string bedNumber, string doctorUsername)
        {
            var doctor = _userRepository.Get(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            var patient = _patientRepository.Get(p => p.PatientId == patientId);
            if (patient != null)
            {
                patient.Status = "ADMITTED";
                patient.AssignedDoctorUsername = doctorUsername;
                patient.AssignedDoctorName = doctorName;
                _patientRepository.Update(patient);
            }

            var admission = new Admission
            {
                AdmissionId = $"ADM{_admissionRepository.GetAll().Count() + 1}",
                PatientId = patientId,
                AdmissionDate = DateTime.Now,
                Ward = ward,
                BedNumber = bedNumber,
                Status = "ADMITTED",
                AssignedDoctorUsername = doctorUsername,
                AssignedDoctorName = doctorName
            };
            _admissionRepository.Add(admission);
            _admissionRepository.SaveChanges();
            return admission;
        }

        public void DeleteAdmission(string admissionId)
        {
            var adm = _admissionRepository.Get(a => a.AdmissionId == admissionId);
            if (adm != null)
            {
                _admissionRepository.Delete(adm);
                _admissionRepository.SaveChanges();
            }
        }

        public void UpdateAdmission(Admission admission)
        {
            var existing = _admissionRepository.Get(a => a.AdmissionId == admission.AdmissionId);
            if (existing != null)
            {
                existing.Ward = admission.Ward;
                existing.BedNumber = admission.BedNumber;
                existing.Status = admission.Status;
                existing.DischargeDate = admission.DischargeDate;
                existing.AssignedDoctorUsername = admission.AssignedDoctorUsername;
                existing.AssignedDoctorName = admission.AssignedDoctorName;
                _admissionRepository.Update(existing);
                _admissionRepository.SaveChanges();
            }
        }

        public void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email)
        {
            var doc = _userRepository.Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            if (doc != null)
            {
                doc.Specialty = specialty;
                doc.Biography = biography;
                doc.ContactNumber = contactNumber;
                doc.Email = email;
                _userRepository.Update(doc);
                _userRepository.SaveChanges();
            }
        }

        public void UpdateUserProfile(string username, string fullName, string contactNumber, string email)
        {
            var user = _userRepository.Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user != null)
            {
                user.FullName = fullName;
                user.ContactNumber = contactNumber;
                user.Email = email;
                _userRepository.Update(user);
                _userRepository.SaveChanges();
            }
        }

        public void AssignDoctorToPatient(string patientId, string doctorUsername)
        {
            var doctor = _userRepository.Get(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            var doctorName = doctor?.FullName ?? "Unknown Doctor";

            var patient = _patientRepository.Get(p => p.PatientId == patientId);
            if (patient != null)
            {
                patient.AssignedDoctorUsername = doctorUsername;
                patient.AssignedDoctorName = doctorName;
                _patientRepository.Update(patient);
            }

            var admission = _admissionRepository.Get(a => a.PatientId == patientId && a.Status == "ADMITTED");
            if (admission != null)
            {
                admission.AssignedDoctorUsername = doctorUsername;
                admission.AssignedDoctorName = doctorName;
                _admissionRepository.Update(admission);
            }
            _patientRepository.SaveChanges();
        }

        // --- EHR RECORDS ---
        public List<EhrRecord> GetEhrRecords()
        {
            return _ehrRepository.GetAll().ToList();
        }

        public List<EhrRecord> GetEhrForPatient(string patientId)
        {
            return _ehrRepository.Find(e => e.PatientId == patientId).ToList();
        }

        public EhrRecord CreateEhr(EhrRecord record)
        {
            record.EhrId = $"EHR00{_ehrRepository.GetAll().Count() + 1}";
            record.VisitDate = DateTime.Now;
            _ehrRepository.Add(record);
            _ehrRepository.SaveChanges();
            return record;
        }

        // --- LAB ORDERS ---
        public List<LabOrder> GetLabOrders()
        {
            return _labOrderRepository.GetAll().ToList();
        }

        public List<LabOrder> GetLabOrdersForPatient(string patientId)
        {
            return _labOrderRepository.Find(l => l.PatientId == patientId).ToList();
        }

        public LabOrder CreateLabOrder(LabOrder order)
        {
            order.LabOrderId = $"LAB{_labOrderRepository.GetAll().Count() + 1}";
            order.Status = "ORDERED";
            order.OrderDate = DateTime.Now;
            _labOrderRepository.Add(order);
            _labOrderRepository.SaveChanges();
            return order;
        }

        public void UpdateLabOrderStatus(string orderId, string status, string result, string technicianName)
        {
            var order = _labOrderRepository.Get(o => o.LabOrderId == orderId);
            if (order != null)
            {
                order.Status = status;
                if (status == "COMPLETED")
                {
                    order.Result = result;
                    order.TechnicianName = technicianName;
                }
                _labOrderRepository.Update(order);
                _labOrderRepository.SaveChanges();
            }
        }

        public void DeleteLabOrder(string orderId)
        {
            var order = _labOrderRepository.Get(o => o.LabOrderId == orderId);
            if (order != null)
            {
                _labOrderRepository.Delete(order);
                _labOrderRepository.SaveChanges();
            }
        }

        // --- TREATMENT PLANS ---
        public List<TreatmentPlan> GetTreatments()
        {
            return _treatmentRepository.GetAll().ToList();
        }

        public List<TreatmentPlan> GetTreatmentsForPatient(string patientId)
        {
            return _treatmentRepository.Find(t => t.PatientId == patientId).ToList();
        }

        public TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan)
        {
            plan.TreatmentPlanId = $"TX{_treatmentRepository.GetAll().Count() + 1}";
            plan.PrescribedDate = DateTime.Now;
            _treatmentRepository.Add(plan);

            // Automatically generate a pharmacy record for dispensing!
            if (!string.IsNullOrWhiteSpace(plan.Medication))
            {
                var medicines = plan.Medication.Split(',');
                foreach (var med in medicines)
                {
                    var medClean = med.Trim();
                    if (string.IsNullOrEmpty(medClean)) continue;
                    
                    _pharmacyRepository.Add(new PharmacyRecord
                    {
                        PharmacyRecordId = $"PHM{_pharmacyRepository.GetAll().Count() + 1}",
                        PatientId = plan.PatientId,
                        TreatmentPlanId = plan.TreatmentPlanId,
                        MedicineName = medClean,
                        Quantity = 10, // Default dosage count
                        Status = "PENDING"
                    });
                }
            }

            _treatmentRepository.SaveChanges();
            return plan;
        }

        public void DeleteTreatmentPlan(string planId)
        {
            var plan = _treatmentRepository.Get(t => t.TreatmentPlanId == planId);
            if (plan != null)
            {
                _treatmentRepository.Delete(plan);
                _treatmentRepository.SaveChanges();
            }
        }

        public void UpdateTreatmentPlan(TreatmentPlan plan)
        {
            var existing = _treatmentRepository.Get(t => t.TreatmentPlanId == plan.TreatmentPlanId);
            if (existing != null)
            {
                existing.Diagnosis = plan.Diagnosis;
                existing.TreatmentDescription = plan.TreatmentDescription;
                existing.Medication = plan.Medication;
                existing.Duration = plan.Duration;
                existing.Instructions = plan.Instructions;
                _treatmentRepository.Update(existing);
                _treatmentRepository.SaveChanges();
            }
        }

        // --- PHARMACY ---
        public List<PharmacyRecord> GetPharmacyRecords()
        {
            return _pharmacyRepository.GetAll().ToList();
        }

        public void DispenseMedicine(string pharmacyRecordId, string pharmacistName)
        {
            var record = _pharmacyRepository.Get(r => r.PharmacyRecordId == pharmacyRecordId);
            if (record != null && record.Status == "PENDING")
            {
                record.Status = "DISPENSED";
                record.DispensedDate = DateTime.Now;
                record.PharmacistName = pharmacistName;
                _pharmacyRepository.Update(record);

                // Deduct stock if exists
                var stock = _medicineStockRepository.Get(m => m.MedicineName == record.MedicineName);
                if (stock != null)
                {
                    stock.Quantity = Math.Max(0, stock.Quantity - record.Quantity);
                    _medicineStockRepository.Update(stock);
                }
                _pharmacyRepository.SaveChanges();
            }
        }

        public void UpdateMedicineStock(string medicineName, int amount)
        {
            if (string.IsNullOrWhiteSpace(medicineName)) return;
            
            string key = medicineName.Trim();
            var existing = _medicineStockRepository.Get(m => m.MedicineName == key);
            if (existing != null)
            {
                existing.Quantity = Math.Max(0, existing.Quantity + amount);
                _medicineStockRepository.Update(existing);
            }
            else
            {
                _medicineStockRepository.Add(new MedicineStock { MedicineName = key, Quantity = Math.Max(0, amount) });
            }
            _medicineStockRepository.SaveChanges();
        }

        public void DeletePharmacyRecord(string pharmacyRecordId)
        {
            var record = _pharmacyRepository.Get(r => r.PharmacyRecordId == pharmacyRecordId);
            if (record != null)
            {
                _pharmacyRepository.Delete(record);
                _pharmacyRepository.SaveChanges();
            }
        }

        // --- BILLING & DISCHARGE (Merged RBAC) ---
        public List<BillingRecord> GetBillingRecords()
        {
            return _billingRepository.GetAll().ToList();
        }

        public BillingRecord? GetBillingForPatient(string patientId)
        {
            return _billingRepository.Get(b => b.PatientId == patientId);
        }

        public BillingRecord GenerateBill(string patientId, decimal consultation, decimal lab, decimal medicine, decimal room)
        {
            var existing = _billingRepository.Get(b => b.PatientId == patientId);
            if (existing != null && existing.Status == "PENDING")
            {
                // Update existing pending bill
                existing.ConsultationFee = consultation;
                existing.LabCharges = lab;
                existing.MedicineCharges = medicine;
                existing.RoomCharges = room;
                _billingRepository.Update(existing);
                _billingRepository.SaveChanges();
                return existing;
            }

            var bill = new BillingRecord
            {
                BillingRecordId = $"BIL00{_billingRepository.GetAll().Count() + 1}",
                PatientId = patientId,
                ConsultationFee = consultation,
                LabCharges = lab,
                MedicineCharges = medicine,
                RoomCharges = room,
                Status = "PENDING",
                CreatedDate = DateTime.Now
            };
            _billingRepository.Add(bill);
            _billingRepository.SaveChanges();
            return bill;
        }

        public void ProcessPayment(string billingRecordId, string paymentMethod = "UPI", string transactionId = "")
        {
            var bill = _billingRepository.Get(b => b.BillingRecordId == billingRecordId);
            if (bill != null)
            {
                bill.Status = "PAID";
                bill.PaymentDate = DateTime.Now;
                bill.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Standard Payment" : paymentMethod;
                bill.TransactionId = string.IsNullOrWhiteSpace(transactionId) ? $"TXN-{DateTime.Now:yyyyMMddHHmmss}" : transactionId;
                _billingRepository.Update(bill);
                _billingRepository.SaveChanges();
            }
        }

        public void DeleteBillingRecord(string billingRecordId)
        {
            var bill = _billingRepository.Get(b => b.BillingRecordId == billingRecordId);
            if (bill != null)
            {
                _billingRepository.Delete(bill);
                _billingRepository.SaveChanges();
            }
        }

        public void DischargePatient(string patientId, string remarks)
        {
            // 1. Update Patient Status
            var patient = _patientRepository.Get(p => p.PatientId == patientId);
            if (patient != null)
            {
                patient.Status = "DISCHARGED";
                _patientRepository.Update(patient);
            }

            // 2. Update Admission details
            var admission = _admissionRepository.Get(a => a.PatientId == patientId && a.Status == "ADMITTED");
            if (admission != null)
            {
                admission.Status = "DISCHARGED";
                admission.DischargeDate = DateTime.Now;
                _admissionRepository.Update(admission);
            }

            // 3. Update Billing Record (Merged RBAC state)
            var bill = _billingRepository.Get(b => b.PatientId == patientId);
            if (bill != null)
            {
                bill.IsDischarged = true;
                bill.DischargeDate = DateTime.Now;
                bill.DischargeRemarks = remarks;
                _billingRepository.Update(bill);
            }

            // 4. Create Discharge Record Log
            var discharge = new DischargeRecord
            {
                DischargeRecordId = $"DIS00{_dischargeRepository.GetAll().Count() + 1}",
                PatientId = patientId,
                AdmissionDate = admission?.AdmissionDate ?? DateTime.Now.AddDays(-1),
                DischargeDate = DateTime.Now,
                Remarks = remarks,
                Summary = $"Discharged by Billing Officer. Bill settled. Patient status set to DISCHARGED. Remarks: {remarks}"
            };
            _dischargeRepository.Add(discharge);
            _dischargeRepository.SaveChanges();
        }

        public List<DischargeRecord> GetDischargeRecords()
        {
            return _dischargeRepository.GetAll().ToList();
        }
    }
}
