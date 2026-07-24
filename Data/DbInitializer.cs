using System;
using System.Linq;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HospitalDbContext context)
        {
            // Self-healing: Update any existing users with blank/empty passwords to "password"
            var existingUsers = context.Users.ToList();
            bool updated = false;
            foreach (var u in existingUsers)
            {
                if (string.IsNullOrEmpty(u.Password))
                {
                    u.Password = "password";
                    updated = true;
                }
            }
            if (updated)
            {
                context.SaveChanges();
            }

            // Self-healing: Seed any missing medicine stocks with high stock quantity
            var defaultMedicines = new System.Collections.Generic.Dictionary<string, int>
            {
                // Cardiology
                { "Atorvastatin 20mg", 500 },
                { "Metoprolol 50mg", 500 },
                { "Clopidogrel 75mg", 500 },
                { "Aspirin 81mg", 500 },
                { "Amlodipine 5mg", 500 },
                // Pediatrics
                { "Amoxicillin 250mg Suspension", 500 },
                { "Paracetamol 120mg Syrup", 500 },
                { "Ibuprofen 100mg Suspension", 500 },
                { "Cetirizine 5mg Syrup", 500 },
                // Orthopedics
                { "Diclofenac 50mg", 500 },
                { "Ibuprofen 400mg", 500 },
                { "Tramadol 50mg", 500 },
                { "Calcium + Vitamin D3", 500 },
                // Neurology
                { "Gabapentin 300mg", 500 },
                { "Levetiracetam 500mg", 500 },
                { "Donepezil 5mg", 500 },
                { "Sumatriptan 50mg", 500 },
                // General Physician / Fallback
                { "Amoxicillin 500mg", 500 },
                { "Paracetamol 650mg", 500 },
                { "Azithromycin 500mg", 500 },
                { "Pantoprazole 40mg", 500 },
                { "Cetirizine 10mg", 500 }
            };

            bool medicineUpdated = false;
            foreach (var kv in defaultMedicines)
            {
                var existingMed = context.MedicineStocks.FirstOrDefault(m => m.MedicineName == kv.Key);
                if (existingMed == null)
                {
                    context.MedicineStocks.Add(new MedicineStock { MedicineName = kv.Key, Quantity = kv.Value });
                    medicineUpdated = true;
                }
                else if (existingMed.Quantity < 500)
                {
                    existingMed.Quantity = 500;
                    medicineUpdated = true;
                }
            }
            if (medicineUpdated)
            {
                context.SaveChanges();
            }

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            // 1. Seed Users
            var admin = new User { Username = "admin", Role = "admin", FullName = "Dr. Aditya Sen", Password = "password" };
            var rec1 = new User { Username = "receptionist", Role = "receptionist", FullName = "Bhanu Prasad(Front Desk Receptionist 1)", Password = "password" };
            var rec2 = new User { Username = "receptionist2", Role = "receptionist", FullName = "Rajesh (Admissions Desk Receptionist 2)", Password = "password" };

            var docRamesh = new User { Username = "Doctor Ramesh", Role = "doctor", FullName = "Dr. Ramesh", DoctorUniqueId = "D001", Specialty = "Cardiology", Biography = "Consultant Cardiologist with over 15 years of experience.", ContactNumber = "9876543001", Email = "harsha@cogmedi.com", Password = "password" };
            var docPriya = new User { Username = "Doctor Priya", Role = "doctor", FullName = "Dr. Priya Sharma", DoctorUniqueId = "D002", Specialty = "Pediatrics", Biography = "Senior Pediatrician specializing in neonatology.", ContactNumber = "9876543002", Email = "priya@cogmedi.com", Password = "password" };
            var docAmit = new User { Username = "Doctor Amit", Role = "doctor", FullName = "Dr. Amit Verma", DoctorUniqueId = "D003", Specialty = "Orthopedics", Biography = "Orthopedic Surgeon focusing on joint replacements.", ContactNumber = "9876543003", Email = "amit@cogmedi.com", Password = "password" };
            var docShalini = new User { Username = "Doctor Shalini", Role = "doctor", FullName = "Dr. Shalini Gupta", DoctorUniqueId = "D004", Specialty = "Neurology", Biography = "Consultant Neurologist. Expert in neurodegenerative disorders.", ContactNumber = "9876543004", Email = "shalini@cogmedi.com", Password = "password" };

            var lab = new User { Username = "lab", Role = "laboratory", FullName = "Karan Malhotra", Password = "password" };
            var pharmacist = new User { Username = "pharmacist", Role = "pharmiacist", FullName = "Rahul Verma", Password = "password" };
            var billing1 = new User { Username = "billing", Role = "billing discharge", FullName = "Ramesh Varma (Senior Billing Officer 1)", Password = "password" };
            var billing2 = new User { Username = "billing2", Role = "billing discharge", FullName = "Kavita Reddy (Discharge Clearance Officer 2)", Password = "password" };

            context.Users.AddRange(admin, rec1, rec2, docRamesh, docPriya, docAmit, docShalini, lab, pharmacist, billing1, billing2);
            context.SaveChanges(); // DB assigns real IDs here

            // 2. Seed Patients
            var p1 = new Patient { Name = "Amit Sharma", Age = 42, Gender = "Male", Address = "Ameerpet, Hyderabad", ContactNumber = "9876543211", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-5) };
            var p2 = new Patient { Name = "Priya Patel", Age = 28, Gender = "Female", Address = "Gachibowli, Hyderabad", ContactNumber = "9876543212", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-2) };
            var p3 = new Patient { Name = "Rajesh Kumar", Age = 55, Gender = "Male", Address = "Kukatpally, Hyderabad", ContactNumber = "9876543213", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma", CreatedDate = DateTime.Now.AddDays(-1) };
            var p4 = new Patient { Name = "Sunitha Rao", Age = 63, Gender = "Female", Address = "Secunderabad, Hyderabad", ContactNumber = "9876543214", Status = "REGISTERED", CreatedDate = DateTime.Now };

            context.Patients.AddRange(p1, p2, p3, p4);
            context.SaveChanges();

            // 3. Seed Admissions (ADDED PatientName)
            var admissions = new[]
            {
                new Admission { PatientId = p1.PatientId, PatientName = p1.Name, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-12", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p2.PatientId, PatientName = p2.Name, AdmissionDate = DateTime.Now.AddDays(-2), Ward = "ICU", BedNumber = "ICU-03", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p3.PatientId, PatientName = p3.Name, AdmissionDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-15", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma" }
            };
            context.Admissions.AddRange(admissions);

            // 4. Seed EHR Records (ADDED PatientName)
            var ehrRecords = new[]
            {
                new EhrRecord { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", DoctorNotes = "Patient complained of severe lower abdominal pain.", VisitDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", DoctorNotes = "Chest congestion, high fever.", VisitDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p3.PatientId, PatientName = p3.Name, DoctorId = docPriya.Id, Diagnosis = "Chronic Hypertension", DoctorNotes = "B/P measured 160/100.", VisitDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.EhrRecords.AddRange(ehrRecords);

            // 5. Seed Order Test Labs (ADDED PatientName)
            var labOrders = new[]
            {
                new OrderTestLab { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, TestName = "Ultrasound Abdomen", Status = "COMPLETED", Result = "Inflamed appendix observed.", OrderDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new OrderTestLab { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, TestName = "Chest X-Ray", Status = "COMPLETED", Result = "Infiltration in lower left lung lobe.", OrderDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new OrderTestLab { PatientId = p3.PatientId, PatientName = p3.Name, DoctorId = docPriya.Id, TestName = "CBC Test", Status = "ORDERED", Result = "", OrderDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.OrderTestLabs.AddRange(labOrders);

            // 6. Seed Treatment Plans (ADDED PatientName)
            var tp1 = new TreatmentPlan { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", TreatmentDescription = "Surgical removal of appendix.", Medication = "Amoxicillin 500mg", Duration = "7 Days", Instructions = "Take twice a day.", PrescribedDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" };
            var tp2 = new TreatmentPlan { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", TreatmentDescription = "IV fluids and nebulizer therapy.", Medication = "Azithromycin 500mg", Duration = "5 Days", Instructions = "Once daily.", PrescribedDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" };

            context.TreatmentPlans.AddRange(tp1, tp2);
            context.SaveChanges();

            // 7. Seed Pharmacy Records (ADDED PatientName)
            var pharmacyRecords = new[]
            {
                new PharmacyRecord { PatientId = p1.PatientId, PatientName = p1.Name, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Amoxicillin 500mg", Quantity = 14, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p1.PatientId, PatientName = p1.Name, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Paracetamol 650mg", Quantity = 10, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p2.PatientId, PatientName = p2.Name, TreatmentPlanId = tp2.TreatmentPlanId, MedicineName = "Azithromycin 500mg", Quantity = 5, Status = "PENDING" }
            };
            context.PharmacyRecords.AddRange(pharmacyRecords);

            // 8. Seed Billing Records (ADDED PatientName)
            var billingRecords = new[]
            {
                new BillingRecord { PatientId = p1.PatientId, PatientName = p1.Name, ConsultationFee = 500, LabCharges = 1500, MedicineCharges = 450, RoomCharges = 3000, Status = "PAID", CreatedDate = DateTime.Now.AddDays(-1), PaymentDate = DateTime.Now.AddDays(-1), IsDischarged = true, DischargeDate = DateTime.Now.AddDays(-1), DischargeRemarks = "Recovered fully." },
                new BillingRecord { PatientId = p2.PatientId, PatientName = p2.Name, ConsultationFee = 500, LabCharges = 1200, MedicineCharges = 250, RoomCharges = 6000, Status = "PENDING", CreatedDate = DateTime.Now }
            };
            context.BillingRecords.AddRange(billingRecords);

            // 9. Seed Discharge Records (ADDED PatientName)
            var dischargeRecords = new[]
            {
                new DischargeRecord { PatientId = p1.PatientId, PatientName = p1.Name, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Remarks = "Recovered fully.", Summary = "Successful laparoscopic appendectomy." }
            };
            context.DischargeRecords.AddRange(dischargeRecords);

            // 10. Seed Medicine Stocks
            var medicineStocks = new[]
            {
                new MedicineStock { MedicineName = "Amoxicillin 500mg", Quantity = 120 },
                new MedicineStock { MedicineName = "Paracetamol 650mg", Quantity = 350 },
                new MedicineStock { MedicineName = "Azithromycin 500mg", Quantity = 80 },
                new MedicineStock { MedicineName = "Pantoprazole 40mg", Quantity = 200 }
            };
            context.MedicineStocks.AddRange(medicineStocks);

            // Final save
            context.SaveChanges();
        }
    }
}