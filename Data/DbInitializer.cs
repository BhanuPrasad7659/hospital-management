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

            // 1. Seed Users
            var admin = new User { Username = "admin", Role = "admin", FullName = "Dr. Aditya Sen" };
            var rec1 = new User { Username = "receptionist", Role = "receptionist", FullName = "Bhanu Prasad(Front Desk Receptionist 1)" };
            var rec2 = new User { Username = "receptionist2", Role = "receptionist", FullName = "Rajesh (Admissions Desk Receptionist 2)" };
            var docRamesh = new User { Username = "Doctor Ramesh", Role = "doctor", FullName = "Dr. Ramesh", Specialty = "Cardiology", Biography = "Consultant Cardiologist with over 15 years of experience. Completed MD at AIIMS.", ContactNumber = "9876543001", Email = "harsha@cogmedi.com" };
            var docPriya = new User { Username = "Doctor Priya", Role = "doctor", FullName = "Dr. Priya Sharma", Specialty = "Pediatrics", Biography = "Senior Pediatrician specializing in neonatology and pediatric emergency care.", ContactNumber = "9876543002", Email = "priya@cogmedi.com" };
            var docAmit = new User { Username = "Doctor Amit", Role = "doctor", FullName = "Dr. Amit Verma", Specialty = "Orthopedics", Biography = "Orthopedic Surgeon focusing on joint replacements and sports medicine.", ContactNumber = "9876543003", Email = "amit@cogmedi.com" };
            var docShalini = new User { Username = "Doctor Shalini", Role = "doctor", FullName = "Dr. Shalini Gupta", Specialty = "Neurology", Biography = "Consultant Neurologist. Expert in neurodegenerative disorders.", ContactNumber = "9876543004", Email = "shalini@cogmedi.com" };
            var lab = new User { Username = "lab", Role = "laboratory", FullName = "Karan Malhotra" };
            var pharmacist = new User { Username = "pharmacist", Role = "pharmiacist", FullName = "Rahul Verma" };
            var billing1 = new User { Username = "billing", Role = "billing discharge", FullName = "Ramesh Varma (Senior Billing Officer 1)" };
            var billing2 = new User { Username = "billing2", Role = "billing discharge", FullName = "Kavita Reddy (Discharge Clearance Officer 2)" };

            context.Users.AddRange(admin, rec1, rec2, docRamesh, docPriya, docAmit, docShalini, lab, pharmacist, billing1, billing2);
            context.SaveChanges(); // DB assigns real IDs here

            // 2. Seed Patients (Using the DB-generated Doctor IDs)
            var p1 = new Patient { Name = "Amit Sharma", Age = 42, Gender = "Male", Address = "Ameerpet, Hyderabad", ContactNumber = "9876543211", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-5) };
            var p2 = new Patient { Name = "Priya Patel", Age = 28, Gender = "Female", Address = "Gachibowli, Hyderabad", ContactNumber = "9876543212", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-2) };
            var p3 = new Patient { Name = "Rajesh Kumar", Age = 55, Gender = "Male", Address = "Kukatpally, Hyderabad", ContactNumber = "9876543213", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma", CreatedDate = DateTime.Now.AddDays(-1) };
            var p4 = new Patient { Name = "Sunitha Rao", Age = 63, Gender = "Female", Address = "Secunderabad, Hyderabad", ContactNumber = "9876543214", Status = "REGISTERED", CreatedDate = DateTime.Now };

            context.Patients.AddRange(p1, p2, p3, p4);
            context.SaveChanges(); // DB assigns real Patient IDs here

            // 3. Seed Admissions (Using real Patient IDs and Doctor IDs)
            var admissions = new[]
            {
                new Admission { PatientId = p1.PatientId, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-12", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p2.PatientId, AdmissionDate = DateTime.Now.AddDays(-2), Ward = "ICU", BedNumber = "ICU-03", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p3.PatientId, AdmissionDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-15", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma" }
            };
            context.Admissions.AddRange(admissions);

            // 4. Seed EHR Records
            var ehrRecords = new[]
            {
                new EhrRecord { PatientId = p1.PatientId, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", DoctorNotes = "Patient complained of severe lower abdominal pain. Recommended appendectomy.", VisitDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p2.PatientId, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", DoctorNotes = "Chest congestion, high fever, and difficulty breathing. Advised ICU admission and IV antibiotics.", VisitDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p3.PatientId, DoctorId = docPriya.Id, Diagnosis = "Chronic Hypertension", DoctorNotes = "B/P measured 160/100. Routine monitoring required. Ordered CBC and ECG.", VisitDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.EhrRecords.AddRange(ehrRecords);

            // 5. Seed Lab Orders
            var labOrders = new[]
            {
                new LabOrder { PatientId = p1.PatientId, DoctorId = docRamesh.Id, TestName = "Ultrasound Abdomen", Status = "COMPLETED", Result = "Inflamed appendix observed. Confirmed appendicitis.", OrderDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new LabOrder { PatientId = p2.PatientId, DoctorId = docRamesh.Id, TestName = "Chest X-Ray", Status = "COMPLETED", Result = "Infiltration in lower left lung lobe. Pattern matches Pneumonia.", OrderDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new LabOrder { PatientId = p3.PatientId, DoctorId = docPriya.Id, TestName = "CBC Test", Status = "ORDERED", Result = "", OrderDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.LabOrders.AddRange(labOrders);

            // 6. Seed Treatment Plans (Captured as variables to link to Pharmacy later)
            var tp1 = new TreatmentPlan { PatientId = p1.PatientId, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", TreatmentDescription = "Surgical removal of appendix followed by post-op care.", Medication = "Amoxicillin 500mg, Paracetamol 650mg", Duration = "7 Days", Instructions = "Take antibiotics twice a day. Painkiller as needed.", PrescribedDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" };
            var tp2 = new TreatmentPlan { PatientId = p2.PatientId, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", TreatmentDescription = "IV fluids and nebulizer therapy.", Medication = "Azithromycin 500mg, Levosalbutamol Inhaler", Duration = "5 Days", Instructions = "Azithromycin once daily, inhaler every 6 hours.", PrescribedDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" };

            context.TreatmentPlans.AddRange(tp1, tp2);
            context.SaveChanges(); // DB assigns real TreatmentPlan IDs here

            // 7. Seed Pharmacy Records (Using real TreatmentPlan IDs)
            var pharmacyRecords = new[]
            {
                new PharmacyRecord { PatientId = p1.PatientId, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Amoxicillin 500mg", Quantity = 14, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p1.PatientId, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Paracetamol 650mg", Quantity = 10, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p2.PatientId, TreatmentPlanId = tp2.TreatmentPlanId, MedicineName = "Azithromycin 500mg", Quantity = 5, Status = "PENDING" }
            };
            context.PharmacyRecords.AddRange(pharmacyRecords);

            // 8. Seed Billing Records
            var billingRecords = new[]
            {
                new BillingRecord { PatientId = p1.PatientId, ConsultationFee = 500, LabCharges = 1500, MedicineCharges = 450, RoomCharges = 3000, Status = "PAID", CreatedDate = DateTime.Now.AddDays(-1), PaymentDate = DateTime.Now.AddDays(-1), IsDischarged = true, DischargeDate = DateTime.Now.AddDays(-1), DischargeRemarks = "Recovered fully. Safe to travel." },
                new BillingRecord { PatientId = p2.PatientId, ConsultationFee = 500, LabCharges = 1200, MedicineCharges = 250, RoomCharges = 6000, Status = "PENDING", CreatedDate = DateTime.Now }
            };
            context.BillingRecords.AddRange(billingRecords);

            // 9. Seed Discharge Records
            var dischargeRecords = new[]
            {
                new DischargeRecord { PatientId = p1.PatientId, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Remarks = "Recovered fully.", Summary = "Successful laparoscopic appendectomy. Post-op recovery uneventful. Vitals stable. Advised rest for 1 week." }
            };
            context.DischargeRecords.AddRange(dischargeRecords);

            // 10. Seed Medicine Stocks (No Foreign Keys, safe to add normally)
            var medicineStocks = new[]
            {
                new MedicineStock { MedicineName = "Amoxicillin 500mg", Quantity = 120 },
                new MedicineStock { MedicineName = "Paracetamol 650mg", Quantity = 350 },
                new MedicineStock { MedicineName = "Azithromycin 500mg", Quantity = 80 },
                new MedicineStock { MedicineName = "Pantoprazole 40mg", Quantity = 200 },
                new MedicineStock { MedicineName = "Metformin 500mg", Quantity = 500 },
                new MedicineStock { MedicineName = "Atorvastatin 10mg", Quantity = 150 }
            };
            context.MedicineStocks.AddRange(medicineStocks);

            // Final save for all remaining records
            context.SaveChanges();
        }
    }
}