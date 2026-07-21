using System;
using System.Linq;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HospitalDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            // 1. Seed Users (Doctor IDs will be 4, 5, 6, 7)
            var users = new[]
            {
                new User { Id = 1, Username = "admin", Role = "admin", FullName = "Dr. Aditya Sen" },
                new User { Id = 2, Username = "receptionist", Role = "receptionist", FullName = "Bhanu Prasad(Front Desk Receptionist 1)" },
                new User { Id = 3, Username = "receptionist2", Role = "receptionist", FullName = "Rajesh (Admissions Desk Receptionist 2)" },
                new User { 
                    Id = 4,
                    Username = "Doctor Ramesh", 
                    Role = "doctor", 
                    FullName = "Dr. Ramesh", 
                    Specialty = "Cardiology", 
                    Biography = "Consultant Cardiologist with over 15 years of experience. Completed MD at AIIMS.", 
                    ContactNumber = "9876543001", 
                    Email = "harsha@cogmedi.com" 
                },
                new User { 
                    Id = 5,
                    Username = "Doctor Priya", 
                    Role = "doctor", 
                    FullName = "Dr. Priya Sharma", 
                    Specialty = "Pediatrics", 
                    Biography = "Senior Pediatrician specializing in neonatology and pediatric emergency care.", 
                    ContactNumber = "9876543002", 
                    Email = "priya@cogmedi.com" 
                },
                new User { 
                    Id = 6,
                    Username = "Doctor Amit", 
                    Role = "doctor", 
                    FullName = "Dr. Amit Verma", 
                    Specialty = "Orthopedics", 
                    Biography = "Orthopedic Surgeon focusing on joint replacements and sports medicine.", 
                    ContactNumber = "9876543003", 
                    Email = "amit@cogmedi.com" 
                },
                new User { 
                    Id = 7,
                    Username = "Doctor Shalini", 
                    Role = "doctor", 
                    FullName = "Dr. Shalini Gupta", 
                    Specialty = "Neurology", 
                    Biography = "Consultant Neurologist. Expert in neurodegenerative disorders.", 
                    ContactNumber = "9876543004", 
                    Email = "shalini@cogmedi.com" 
                },
                new User { Id = 8, Username = "lab", Role = "laboratory", FullName = "Karan Malhotra" },
                new User { Id = 9, Username = "pharmacist", Role = "pharmiacist", FullName = "Rahul Verma" },
                new User { Id = 10, Username = "billing", Role = "billing discharge", FullName = "Ramesh Varma (Senior Billing Officer 1)" },
                new User { Id = 11, Username = "billing2", Role = "billing discharge", FullName = "Kavita Reddy (Discharge Clearance Officer 2)" }
            };
            context.Users.AddRange(users);

            // 2. Seed Patients
            var patients = new[]
            {
                new Patient { PatientId = 1, Name = "Amit Sharma", Age = 42, Gender = "Male", Address = "Ameerpet, Hyderabad", ContactNumber = "9876543211", Status = "DISCHARGED", AssignedDoctorId = 4, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-5) },
                new Patient { PatientId = 2, Name = "Priya Patel", Age = 28, Gender = "Female", Address = "Gachibowli, Hyderabad", ContactNumber = "9876543212", Status = "ADMITTED", AssignedDoctorId = 4, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-2) },
                new Patient { PatientId = 3, Name = "Rajesh Kumar", Age = 55, Gender = "Male", Address = "Kukatpally, Hyderabad", ContactNumber = "9876543213", Status = "ADMITTED", AssignedDoctorId = 5, AssignedDoctorName = "Dr. Priya Sharma", CreatedDate = DateTime.Now.AddDays(-1) },
                new Patient { PatientId = 4, Name = "Sunitha Rao", Age = 63, Gender = "Female", Address = "Secunderabad, Hyderabad", ContactNumber = "9876543214", Status = "REGISTERED", CreatedDate = DateTime.Now }
            };
            context.Patients.AddRange(patients);

            // 3. Seed Admissions
            var admissions = new[]
            {
                new Admission { AdmissionId = 1, PatientId = 1, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-12", Status = "DISCHARGED", AssignedDoctorId = 4, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { AdmissionId = 2, PatientId = 2, AdmissionDate = DateTime.Now.AddDays(-2), Ward = "ICU", BedNumber = "ICU-03", Status = "ADMITTED", AssignedDoctorId = 4, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { AdmissionId = 3, PatientId = 3, AdmissionDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-15", Status = "ADMITTED", AssignedDoctorId = 5, AssignedDoctorName = "Dr. Priya Sharma" }
            };
            context.Admissions.AddRange(admissions);

            // 4. Seed EHR Records
            var ehrRecords = new[]
            {
                new EhrRecord { EhrId = 1, PatientId = 1, DoctorId = 4, Diagnosis = "Acute Appendicitis", DoctorNotes = "Patient complained of severe lower abdominal pain. Recommended appendectomy.", VisitDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" },
                new EhrRecord { EhrId = 2, PatientId = 2, DoctorId = 4, Diagnosis = "Pneumonia", DoctorNotes = "Chest congestion, high fever, and difficulty breathing. Advised ICU admission and IV antibiotics.", VisitDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" },
                new EhrRecord { EhrId = 3, PatientId = 3, DoctorId = 4, Diagnosis = "Chronic Hypertension", DoctorNotes = "B/P measured 160/100. Routine monitoring required. Ordered CBC and ECG.", VisitDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Ramesh" }
            };
            context.EhrRecords.AddRange(ehrRecords);

            // 5. Seed Lab Orders
            var labOrders = new[]
            {
                new LabOrder { LabOrderId = 1, PatientId = 1, DoctorId = 4, TestName = "Ultrasound Abdomen", Status = "COMPLETED", Result = "Inflamed appendix observed. Confirmed appendicitis.", OrderDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new LabOrder { LabOrderId = 2, PatientId = 2, DoctorId = 4, TestName = "Chest X-Ray", Status = "COMPLETED", Result = "Infiltration in lower left lung lobe. Pattern matches Pneumonia.", OrderDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new LabOrder { LabOrderId = 3, PatientId = 3, DoctorId = 4, TestName = "CBC Test", Status = "ORDERED", Result = "", OrderDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Ramesh" }
            };
            context.LabOrders.AddRange(labOrders);

            // 6. Seed Treatment Plans
            var treatmentPlans = new[]
            {
                new TreatmentPlan { TreatmentPlanId = 1, PatientId = 1, DoctorId = 4, Diagnosis = "Acute Appendicitis", TreatmentDescription = "Surgical removal of appendix followed by post-op care.", Medication = "Amoxicillin 500mg, Paracetamol 650mg", Duration = "7 Days", Instructions = "Take antibiotics twice a day. Painkiller as needed.", PrescribedDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" },
                new TreatmentPlan { TreatmentPlanId = 2, PatientId = 2, DoctorId = 4, Diagnosis = "Pneumonia", TreatmentDescription = "IV fluids and nebulizer therapy.", Medication = "Azithromycin 500mg, Levosalbutamol Inhaler", Duration = "5 Days", Instructions = "Azithromycin once daily, inhaler every 6 hours.", PrescribedDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" }
            };
            context.TreatmentPlans.AddRange(treatmentPlans);

            // 7. Seed Pharmacy Records
            var pharmacyRecords = new[]
            {
                new PharmacyRecord { PharmacyRecordId = 1, PatientId = 1, TreatmentPlanId = 1, MedicineName = "Amoxicillin 500mg", Quantity = 14, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PharmacyRecordId = 2, PatientId = 1, TreatmentPlanId = 1, MedicineName = "Paracetamol 650mg", Quantity = 10, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PharmacyRecordId = 3, PatientId = 2, TreatmentPlanId = 2, MedicineName = "Azithromycin 500mg", Quantity = 5, Status = "PENDING" }
            };
            context.PharmacyRecords.AddRange(pharmacyRecords);

            // 8. Seed Billing Records
            var billingRecords = new[]
            {
                new BillingRecord { BillingRecordId = 1, PatientId = 1, ConsultationFee = 500, LabCharges = 1500, MedicineCharges = 450, RoomCharges = 3000, Status = "PAID", CreatedDate = DateTime.Now.AddDays(-1), PaymentDate = DateTime.Now.AddDays(-1), IsDischarged = true, DischargeDate = DateTime.Now.AddDays(-1), DischargeRemarks = "Recovered fully. Safe to travel." },
                new BillingRecord { BillingRecordId = 2, PatientId = 2, ConsultationFee = 500, LabCharges = 1200, MedicineCharges = 250, RoomCharges = 6000, Status = "PENDING", CreatedDate = DateTime.Now }
            };
            context.BillingRecords.AddRange(billingRecords);

            // 9. Seed Discharge Records
            var dischargeRecords = new[]
            {
                new DischargeRecord { DischargeRecordId = 1, PatientId = 1, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Remarks = "Recovered fully.", Summary = "Successful laparoscopic appendectomy. Post-op recovery uneventful. Vitals stable. Advised rest for 1 week." }
            };
            context.DischargeRecords.AddRange(dischargeRecords);

            // 10. Seed Medicine Stocks
            var medicineStocks = new[]
            {
                new MedicineStock { MedicineStockId = 1, MedicineName = "Amoxicillin 500mg", Quantity = 120 },
                new MedicineStock { MedicineStockId = 2, MedicineName = "Paracetamol 650mg", Quantity = 350 },
                new MedicineStock { MedicineStockId = 3, MedicineName = "Azithromycin 500mg", Quantity = 80 },
                new MedicineStock { MedicineStockId = 4, MedicineName = "Pantoprazole 40mg", Quantity = 200 },
                new MedicineStock { MedicineStockId = 5, MedicineName = "Metformin 500mg", Quantity = 500 },
                new MedicineStock { MedicineStockId = 6, MedicineName = "Atorvastatin 10mg", Quantity = 150 }
            };
            context.MedicineStocks.AddRange(medicineStocks);

            context.SaveChanges();
        }
    }
}
