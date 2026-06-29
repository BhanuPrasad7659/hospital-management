using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services
{
    public class HospitalService
    {
        private readonly List<User> _users = new();
        private readonly List<Patient> _patients = new();
        private readonly List<Admission> _admissions = new();
        private readonly List<EhrRecord> _ehrRecords = new();
        private readonly List<LabOrder> _labOrders = new();
        private readonly List<TreatmentPlan> _treatmentPlans = new();
        private readonly List<PharmacyRecord> _pharmacyRecords = new();
        private readonly List<BillingRecord> _billingRecords = new();
        private readonly List<DischargeRecord> _dischargeRecords = new();
        
        // Mock Medicine Stock
        public Dictionary<string, int> MedicineStock { get; } = new();

        private readonly object _lock = new();

        public HospitalService()
        {
            SeedData();
        }

        private void SeedData()
        {
            lock (_lock)
            {
                // 1. Seed Users
                _users.Add(new User { Username = "admin", Role = "admin", FullName = "Dr. Aditya Sen" });
                _users.Add(new User { Username = "receptionist", Role = "receptionist", FullName = "Sita Ramam" });
                _users.Add(new User { Username = "doctor", Role = "doctor", FullName = "Dr. Harsha Vardhan" });
                _users.Add(new User { Username = "lab", Role = "laboratory", FullName = "Karan Malhotra" });
                _users.Add(new User { Username = "pharmacist", Role = "pharmiacist", FullName = "Rahul Verma" });
                _users.Add(new User { Username = "billing", Role = "billing discharge", FullName = "Meera Nair" });

                // 2. Seed Patients
                var p1 = new Patient { PatientId = "P000101", Name = "Amit Sharma", Age = 42, Gender = "Male", Address = "Ameerpet, Hyderabad", ContactNumber = "9876543211", Status = "DISCHARGED" };
                var p2 = new Patient { PatientId = "P000102", Name = "Priya Patel", Age = 28, Gender = "Female", Address = "Gachibowli, Hyderabad", ContactNumber = "9876543212", Status = "ADMITTED" };
                var p3 = new Patient { PatientId = "P000103", Name = "Rajesh Kumar", Age = 55, Gender = "Male", Address = "Kukatpally, Hyderabad", ContactNumber = "9876543213", Status = "ADMITTED" };
                var p4 = new Patient { PatientId = "P000104", Name = "Sunitha Rao", Age = 63, Gender = "Female", Address = "Secunderabad, Hyderabad", ContactNumber = "9876543214", Status = "REGISTERED" };

                _patients.AddRange(new[] { p1, p2, p3, p4 });

                // 3. Seed Admissions
                _admissions.Add(new Admission { AdmissionId = "ADM001", PatientId = "P000101", AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-12", Status = "DISCHARGED" });
                _admissions.Add(new Admission { AdmissionId = "ADM002", PatientId = "P000102", AdmissionDate = DateTime.Now.AddDays(-2), Ward = "ICU", BedNumber = "ICU-03", Status = "ADMITTED" });
                _admissions.Add(new Admission { AdmissionId = "ADM003", PatientId = "P000103", AdmissionDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-15", Status = "ADMITTED" });

                // 4. Seed EHR Records
                _ehrRecords.Add(new EhrRecord { EhrId = "EHR001", PatientId = "P000101", Diagnosis = "Acute Appendicitis", DoctorNotes = "Patient complained of severe lower abdominal pain. Recommended appendectomy.", VisitDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Harsha Vardhan" });
                _ehrRecords.Add(new EhrRecord { EhrId = "EHR002", PatientId = "P000102", Diagnosis = "Pneumonia", DoctorNotes = "Chest congestion, high fever, and difficulty breathing. Advised ICU admission and IV antibiotics.", VisitDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Harsha Vardhan" });
                _ehrRecords.Add(new EhrRecord { EhrId = "EHR003", PatientId = "P000103", Diagnosis = "Chronic Hypertension", DoctorNotes = "B/P measured 160/100. Routine monitoring required. Ordered CBC and ECG.", VisitDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Harsha Vardhan" });

                // 5. Seed Lab Orders
                _labOrders.Add(new LabOrder { LabOrderId = "LAB001", PatientId = "P000101", TestName = "Ultrasound Abdomen", Status = "COMPLETED", Result = "Inflamed appendix observed. Confirmed appendicitis.", OrderDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Harsha Vardhan", TechnicianName = "Karan Malhotra" });
                _labOrders.Add(new LabOrder { LabOrderId = "LAB002", PatientId = "P000102", TestName = "Chest X-Ray", Status = "COMPLETED", Result = "Infiltration in lower left lung lobe. Pattern matches Pneumonia.", OrderDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Harsha Vardhan", TechnicianName = "Karan Malhotra" });
                _labOrders.Add(new LabOrder { LabOrderId = "LAB003", PatientId = "P000103", TestName = "CBC Test", Status = "ORDERED", Result = "", OrderDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Harsha Vardhan" });

                // 6. Seed Treatment Plans
                _treatmentPlans.Add(new TreatmentPlan { TreatmentPlanId = "TX001", PatientId = "P000101", Diagnosis = "Acute Appendicitis", TreatmentDescription = "Surgical removal of appendix followed by post-op care.", Medication = "Amoxicillin 500mg, Paracetamol 650mg", Duration = "7 Days", Instructions = "Take antibiotics twice a day. Painkiller as needed.", PrescribedDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Harsha Vardhan" });
                _treatmentPlans.Add(new TreatmentPlan { TreatmentPlanId = "TX002", PatientId = "P000102", Diagnosis = "Pneumonia", TreatmentDescription = "IV fluids and nebulizer therapy.", Medication = "Azithromycin 500mg, Levosalbutamol Inhaler", Duration = "5 Days", Instructions = "Azithromycin once daily, inhaler every 6 hours.", PrescribedDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Harsha Vardhan" });

                // 7. Seed Pharmacy Records
                _pharmacyRecords.Add(new PharmacyRecord { PharmacyRecordId = "PHM001", PatientId = "P000101", TreatmentPlanId = "TX001", MedicineName = "Amoxicillin 500mg", Quantity = 14, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" });
                _pharmacyRecords.Add(new PharmacyRecord { PharmacyRecordId = "PHM002", PatientId = "P000101", TreatmentPlanId = "TX001", MedicineName = "Paracetamol 650mg", Quantity = 10, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" });
                _pharmacyRecords.Add(new PharmacyRecord { PharmacyRecordId = "PHM003", PatientId = "P000102", TreatmentPlanId = "TX002", MedicineName = "Azithromycin 500mg", Quantity = 5, Status = "PENDING" });

                // 8. Seed Billing Records
                _billingRecords.Add(new BillingRecord { BillingRecordId = "BIL001", PatientId = "P000101", ConsultationFee = 500, LabCharges = 1500, MedicineCharges = 450, RoomCharges = 3000, Status = "PAID", CreatedDate = DateTime.Now.AddDays(-1), PaymentDate = DateTime.Now.AddDays(-1), IsDischarged = true, DischargeDate = DateTime.Now.AddDays(-1), DischargeRemarks = "Recovered fully. Safe to travel." });
                _billingRecords.Add(new BillingRecord { BillingRecordId = "BIL002", PatientId = "P000102", ConsultationFee = 500, LabCharges = 1200, MedicineCharges = 250, RoomCharges = 6000, Status = "PENDING", CreatedDate = DateTime.Now });

                // 9. Seed Discharge Records
                _dischargeRecords.Add(new DischargeRecord { DischargeRecordId = "DIS001", PatientId = "P000101", AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Remarks = "Recovered fully.", Summary = "Successful laparoscopic appendectomy. Post-op recovery uneventful. Vitals stable. Advised rest for 1 week." });

                // 10. Seed Medicine Stocks
                MedicineStock["Amoxicillin 500mg"] = 120;
                MedicineStock["Paracetamol 650mg"] = 350;
                MedicineStock["Azithromycin 500mg"] = 80;
                MedicineStock["Pantoprazole 40mg"] = 200;
                MedicineStock["Metformin 500mg"] = 500;
                MedicineStock["Atorvastatin 10mg"] = 150;
            }
        }

        // --- USER AUTHENTICATION ---
        public User? ValidateUser(string username, string role)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
                if (user == null && !string.IsNullOrEmpty(username))
                {
                    user = new User { Username = username, Role = role, FullName = username };
                    _users.Add(user);
                }
                return user;
            }
        }

        public List<User> GetUsers()
        {
            lock (_lock) return _users.ToList();
        }

        public void AssignStaff(string fullName, string username, string role)
        {
            lock (_lock)
            {
                var existing = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.FullName = fullName;
                    existing.Role = role;
                }
                else
                {
                    _users.Add(new User { FullName = fullName, Username = username, Role = role });
                }
            }
        }

        // --- PATIENTS ---
        public List<Patient> GetPatients()
        {
            lock (_lock) return _patients.ToList();
        }

        public Patient? GetPatient(string patientId)
        {
            lock (_lock) return _patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public Patient RegisterPatient(Patient patient)
        {
            lock (_lock)
            {
                int nextNum = _patients.Count + 101;
                patient.PatientId = $"P000{nextNum}";
                patient.Status = "REGISTERED";
                patient.CreatedDate = DateTime.Now;
                _patients.Add(patient);
                return patient;
            }
        }

        public void UpdatePatient(Patient patient)
        {
            lock (_lock)
            {
                var existing = _patients.FirstOrDefault(p => p.PatientId == patient.PatientId);
                if (existing != null)
                {
                    existing.Name = patient.Name;
                    existing.Age = patient.Age;
                    existing.Gender = patient.Gender;
                    existing.Address = patient.Address;
                    existing.ContactNumber = patient.ContactNumber;
                    existing.Status = patient.Status;
                }
            }
        }

        public void DeletePatient(string patientId)
        {
            lock (_lock)
            {
                var patient = _patients.FirstOrDefault(p => p.PatientId == patientId);
                if (patient != null) _patients.Remove(patient);
            }
        }

        // --- ADMISSIONS ---
        public List<Admission> GetAdmissions()
        {
            lock (_lock) return _admissions.ToList();
        }

        public Admission AdmitPatient(string patientId, string ward, string bedNumber)
        {
            lock (_lock)
            {
                var patient = _patients.FirstOrDefault(p => p.PatientId == patientId);
                if (patient != null)
                {
                    patient.Status = "ADMITTED";
                }

                var admission = new Admission
                {
                    AdmissionId = $"ADM00{_admissions.Count + 1}",
                    PatientId = patientId,
                    AdmissionDate = DateTime.Now,
                    Ward = ward,
                    BedNumber = bedNumber,
                    Status = "ADMITTED"
                };
                _admissions.Add(admission);
                return admission;
            }
        }

        // --- EHR RECORDS ---
        public List<EhrRecord> GetEhrRecords()
        {
            lock (_lock) return _ehrRecords.ToList();
        }

        public List<EhrRecord> GetEhrForPatient(string patientId)
        {
            lock (_lock) return _ehrRecords.Where(e => e.PatientId == patientId).ToList();
        }

        public EhrRecord CreateEhr(EhrRecord record)
        {
            lock (_lock)
            {
                record.EhrId = $"EHR00{_ehrRecords.Count + 1}";
                record.VisitDate = DateTime.Now;
                _ehrRecords.Add(record);
                return record;
            }
        }

        // --- LAB ORDERS ---
        public List<LabOrder> GetLabOrders()
        {
            lock (_lock) return _labOrders.ToList();
        }

        public List<LabOrder> GetLabOrdersForPatient(string patientId)
        {
            lock (_lock) return _labOrders.Where(l => l.PatientId == patientId).ToList();
        }

        public LabOrder CreateLabOrder(LabOrder order)
        {
            lock (_lock)
            {
                order.LabOrderId = $"LAB00{_labOrders.Count + 1}";
                order.Status = "ORDERED";
                order.OrderDate = DateTime.Now;
                _labOrders.Add(order);
                return order;
            }
        }

        public void UpdateLabOrderStatus(string orderId, string status, string result, string technicianName)
        {
            lock (_lock)
            {
                var order = _labOrders.FirstOrDefault(o => o.LabOrderId == orderId);
                if (order != null)
                {
                    order.Status = status;
                    if (status == "COMPLETED")
                    {
                        order.Result = result;
                        order.TechnicianName = technicianName;
                    }
                }
            }
        }

        // --- TREATMENT PLANS ---
        public List<TreatmentPlan> GetTreatments()
        {
            lock (_lock) return _treatmentPlans.ToList();
        }

        public List<TreatmentPlan> GetTreatmentsForPatient(string patientId)
        {
            lock (_lock) return _treatmentPlans.Where(t => t.PatientId == patientId).ToList();
        }

        public TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan)
        {
            lock (_lock)
            {
                plan.TreatmentPlanId = $"TX00{_treatmentPlans.Count + 1}";
                plan.PrescribedDate = DateTime.Now;
                _treatmentPlans.Add(plan);

                // Automatically generate a pharmacy record for dispensing!
                if (!string.IsNullOrWhiteSpace(plan.Medication))
                {
                    var medicines = plan.Medication.Split(',');
                    foreach (var med in medicines)
                    {
                        var medClean = med.Trim();
                        if (string.IsNullOrEmpty(medClean)) continue;
                        
                        _pharmacyRecords.Add(new PharmacyRecord
                        {
                            PharmacyRecordId = $"PHM00{_pharmacyRecords.Count + 1}",
                            PatientId = plan.PatientId,
                            TreatmentPlanId = plan.TreatmentPlanId,
                            MedicineName = medClean,
                            Quantity = 10, // Default dosage count
                            Status = "PENDING"
                        });
                    }
                }

                return plan;
            }
        }

        // --- PHARMACY ---
        public List<PharmacyRecord> GetPharmacyRecords()
        {
            lock (_lock) return _pharmacyRecords.ToList();
        }

        public void DispenseMedicine(string pharmacyRecordId, string pharmacistName)
        {
            lock (_lock)
            {
                var record = _pharmacyRecords.FirstOrDefault(r => r.PharmacyRecordId == pharmacyRecordId);
                if (record != null && record.Status == "PENDING")
                {
                    record.Status = "DISPENSED";
                    record.DispensedDate = DateTime.Now;
                    record.PharmacistName = pharmacistName;

                    // Deduct stock if exists
                    if (MedicineStock.ContainsKey(record.MedicineName))
                    {
                        MedicineStock[record.MedicineName] = Math.Max(0, MedicineStock[record.MedicineName] - record.Quantity);
                    }
                }
            }
        }

        // --- BILLING & DISCHARGE (Merged RBAC) ---
        public List<BillingRecord> GetBillingRecords()
        {
            lock (_lock) return _billingRecords.ToList();
        }

        public BillingRecord? GetBillingForPatient(string patientId)
        {
            lock (_lock) return _billingRecords.FirstOrDefault(b => b.PatientId == patientId);
        }

        public BillingRecord GenerateBill(string patientId, decimal consultation, decimal lab, decimal medicine, decimal room)
        {
            lock (_lock)
            {
                var existing = _billingRecords.FirstOrDefault(b => b.PatientId == patientId);
                if (existing != null && existing.Status == "PENDING")
                {
                    // Update existing pending bill
                    existing.ConsultationFee = consultation;
                    existing.LabCharges = lab;
                    existing.MedicineCharges = medicine;
                    existing.RoomCharges = room;
                    return existing;
                }

                var bill = new BillingRecord
                {
                    BillingRecordId = $"BIL00{_billingRecords.Count + 1}",
                    PatientId = patientId,
                    ConsultationFee = consultation,
                    LabCharges = lab,
                    MedicineCharges = medicine,
                    RoomCharges = room,
                    Status = "PENDING",
                    CreatedDate = DateTime.Now
                };
                _billingRecords.Add(bill);
                return bill;
            }
        }

        public void ProcessPayment(string billingRecordId)
        {
            lock (_lock)
            {
                var bill = _billingRecords.FirstOrDefault(b => b.BillingRecordId == billingRecordId);
                if (bill != null)
                {
                    bill.Status = "PAID";
                    bill.PaymentDate = DateTime.Now;
                }
            }
        }

        public void DischargePatient(string patientId, string remarks)
        {
            lock (_lock)
            {
                // 1. Update Patient Status
                var patient = _patients.FirstOrDefault(p => p.PatientId == patientId);
                if (patient != null)
                {
                    patient.Status = "DISCHARGED";
                }

                // 2. Update Admission details
                var admission = _admissions.FirstOrDefault(a => a.PatientId == patientId && a.Status == "ADMITTED");
                if (admission != null)
                {
                    admission.Status = "DISCHARGED";
                    admission.DischargeDate = DateTime.Now;
                }

                // 3. Update Billing Record (Merged RBAC state)
                var bill = _billingRecords.FirstOrDefault(b => b.PatientId == patientId);
                if (bill != null)
                {
                    bill.IsDischarged = true;
                    bill.DischargeDate = DateTime.Now;
                    bill.DischargeRemarks = remarks;
                }

                // 4. Create Discharge Record Log
                var discharge = new DischargeRecord
                {
                    DischargeRecordId = $"DIS00{_dischargeRecords.Count + 1}",
                    PatientId = patientId,
                    AdmissionDate = admission?.AdmissionDate ?? DateTime.Now.AddDays(-1),
                    DischargeDate = DateTime.Now,
                    Remarks = remarks,
                    Summary = $"Discharged by Billing Officer. Bill settled. Patient status set to DISCHARGED. Remarks: {remarks}"
                };
                _dischargeRecords.Add(discharge);
            }
        }

        public List<DischargeRecord> GetDischargeRecords()
        {
            lock (_lock) return _dischargeRecords.ToList();
        }
    }
}
