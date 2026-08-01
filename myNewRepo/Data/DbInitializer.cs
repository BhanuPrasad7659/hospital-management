using System;
using System.Linq;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(HospitalDbContext context)
        {
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

            var defaultMedicines = new System.Collections.Generic.Dictionary<string, (int Qty, decimal Price)>
            {
                { "Atorvastatin 20mg", (500, 15.50m) },
                { "Metoprolol 50mg", (500, 8.00m) },
                { "Clopidogrel 75mg", (500, 12.00m) },
                { "Aspirin 81mg", (500, 3.00m) },
                { "Amlodipine 5mg", (500, 6.50m) },
                { "Lisinopril 10mg", (500, 9.00m) },
                { "Losartan 50mg", (500, 11.00m) },
                { "Carvedilol 6.25mg", (500, 7.20m) },
                { "Spironolactone 25mg", (500, 14.00m) },
                { "Furosemide 40mg", (500, 4.50m) },

                { "Amoxicillin 250mg Suspension", (500, 45.00m) },
                { "Paracetamol 120mg Syrup", (500, 30.00m) },
                { "Ibuprofen 100mg Suspension", (500, 35.00m) },
                { "Cetirizine 5mg Syrup", (500, 40.00m) },
                { "Zinc Drops 15ml", (500, 55.00m) },
                { "Vitamin D3 Drops", (500, 90.00m) },
                { "Oral Rehydration Salts (ORS)", (500, 15.00m) },
                { "Salbutamol 2mg Syrup", (500, 25.00m) },
                { "Cough Relief Pediatric", (500, 48.00m) },
                { "Multivitamin Pediatric Syrup", (500, 75.00m) },

                { "Diclofenac 50mg", (500, 8.50m) },
                { "Ibuprofen 400mg", (500, 5.00m) },
                { "Tramadol 50mg", (500, 18.00m) },
                { "Calcium + Vitamin D3", (500, 12.50m) },
                { "Methylsulfonylmethane (MSM)", (500, 22.00m) },
                { "Glucosamine Chondroitin", (500, 35.00m) },
                { "Aceclofenac 100mg", (500, 9.50m) },
                { "Etoricoxib 90mg", (500, 24.00m) },
                { "Pregabalin 75mg", (500, 28.00m) },
                { "Paracetamol + Thiocolchicoside", (500, 32.00m) },

                { "Gabapentin 300mg", (500, 22.50m) },
                { "Levetiracetam 500mg", (500, 35.00m) },
                { "Donepezil 5mg", (500, 45.00m) },
                { "Sumatriptan 50mg", (500, 60.00m) },
                { "Methylcobalamin 1500mcg", (500, 15.00m) },
                { "Sodium Valproate 300mg", (500, 18.50m) },
                { "Carbamazepine 200mg", (500, 11.00m) },
                { "Amitriptyline 10mg", (500, 6.00m) },
                { "Topiramate 50mg", (500, 26.00m) },
                { "Clonazepam 0.5mg", (500, 8.00m) },

                { "Amoxicillin 500mg", (500, 10.00m) },
                { "Paracetamol 650mg", (500, 4.00m) },
                { "Azithromycin 500mg", (500, 25.00m) },
                { "Pantoprazole 40mg", (500, 9.00m) },
                { "Cetirizine 10mg", (500, 5.50m) },
                { "Ranitidine 150mg", (500, 3.50m) },
                { "Omeprazole 20mg", (500, 6.00m) },
                { "Dolo 650mg", (500, 4.50m) },
                { "Cough Syrup Adults", (500, 65.00m) },
                { "B-Complex with Zinc", (500, 7.00m) }
            };

            bool medicineUpdated = false;
            foreach (var kv in defaultMedicines)
            {
                var existingMed = context.MedicineStocks.FirstOrDefault(m => m.MedicineName == kv.Key);
                if (existingMed == null)
                {
                    context.MedicineStocks.Add(new MedicineStock 
                    { 
                        MedicineName = kv.Key, 
                        Quantity = kv.Value.Qty, 
                        Price = kv.Value.Price 
                    });
                    medicineUpdated = true;
                }
                else
                {
                    bool modified = false;

                    if (existingMed.Quantity < kv.Value.Qty)
                    {
                        existingMed.Quantity = kv.Value.Qty;
                        modified = true;
                    }

                    if (existingMed.Price != kv.Value.Price)
                    {
                        existingMed.Price = kv.Value.Price;
                        modified = true;
                    }
                    if (modified) medicineUpdated = true;
                }
            }
            if (medicineUpdated)
            {
                context.SaveChanges();
            }

            if (context.Users.Any())
            {
                return;   

            }

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
            context.SaveChanges(); 

            var p1 = new Patient { Name = "Amit Sharma", Age = 42, Gender = "Male", Address = "Ameerpet, Hyderabad", ContactNumber = "9876543211", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-5) };
            var p2 = new Patient { Name = "Priya Patel", Age = 28, Gender = "Female", Address = "Gachibowli, Hyderabad", ContactNumber = "9876543212", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh", CreatedDate = DateTime.Now.AddDays(-2) };
            var p3 = new Patient { Name = "Rajesh Kumar", Age = 55, Gender = "Male", Address = "Kukatpally, Hyderabad", ContactNumber = "9876543213", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma", CreatedDate = DateTime.Now.AddDays(-1) };
            var p4 = new Patient { Name = "Sunitha Rao", Age = 63, Gender = "Female", Address = "Secunderabad, Hyderabad", ContactNumber = "9876543214", Status = "REGISTERED", CreatedDate = DateTime.Now };

            context.Patients.AddRange(p1, p2, p3, p4);
            context.SaveChanges();

            var admissions = new[]
            {
                new Admission { PatientId = p1.PatientId, PatientName = p1.Name, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-12", Status = "DISCHARGED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p2.PatientId, PatientName = p2.Name, AdmissionDate = DateTime.Now.AddDays(-2), Ward = "ICU", BedNumber = "ICU-03", Status = "ADMITTED", AssignedDoctorId = docRamesh.Id, AssignedDoctorName = "Dr. Ramesh" },
                new Admission { PatientId = p3.PatientId, PatientName = p3.Name, AdmissionDate = DateTime.Now.AddDays(-1), Ward = "General Ward", BedNumber = "G-15", Status = "ADMITTED", AssignedDoctorId = docPriya.Id, AssignedDoctorName = "Dr. Priya Sharma" }
            };
            context.Admissions.AddRange(admissions);

            var ehrRecords = new[]
            {
                new EhrRecord { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", DoctorNotes = "Patient complained of severe lower abdominal pain.", VisitDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", DoctorNotes = "Chest congestion, high fever.", VisitDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" },
                new EhrRecord { PatientId = p3.PatientId, PatientName = p3.Name, DoctorId = docPriya.Id, Diagnosis = "Chronic Hypertension", DoctorNotes = "B/P measured 160/100.", VisitDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.EhrRecords.AddRange(ehrRecords);

            var labOrders = new[]
            {
                new OrderTestLab { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, TestName = "Ultrasound Abdomen", Status = "COMPLETED", Result = "Inflamed appendix observed.", OrderDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new OrderTestLab { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, TestName = "Chest X-Ray", Status = "COMPLETED", Result = "Infiltration in lower left lung lobe.", OrderDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh", TechnicianName = "Karan Malhotra" },
                new OrderTestLab { PatientId = p3.PatientId, PatientName = p3.Name, DoctorId = docPriya.Id, TestName = "CBC Test", Status = "ORDERED", Result = "", OrderDate = DateTime.Now.AddDays(-1), DoctorName = "Dr. Priya Sharma" }
            };
            context.OrderTestLabs.AddRange(labOrders);

            var tp1 = new TreatmentPlan { PatientId = p1.PatientId, PatientName = p1.Name, DoctorId = docRamesh.Id, Diagnosis = "Acute Appendicitis", TreatmentDescription = "Surgical removal of appendix.", Medication = "Amoxicillin 500mg", Duration = "7 Days", Instructions = "Take twice a day.", PrescribedDate = DateTime.Now.AddDays(-5), DoctorName = "Dr. Ramesh" };
            var tp2 = new TreatmentPlan { PatientId = p2.PatientId, PatientName = p2.Name, DoctorId = docRamesh.Id, Diagnosis = "Pneumonia", TreatmentDescription = "IV fluids and nebulizer therapy.", Medication = "Azithromycin 500mg", Duration = "5 Days", Instructions = "Once daily.", PrescribedDate = DateTime.Now.AddDays(-2), DoctorName = "Dr. Ramesh" };

            context.TreatmentPlans.AddRange(tp1, tp2);
            context.SaveChanges();

            var pharmacyRecords = new[]
            {
                new PharmacyRecord { PatientId = p1.PatientId, PatientName = p1.Name, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Amoxicillin 500mg", Quantity = 14, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p1.PatientId, PatientName = p1.Name, TreatmentPlanId = tp1.TreatmentPlanId, MedicineName = "Paracetamol 650mg", Quantity = 10, Status = "DISPENSED", DispensedDate = DateTime.Now.AddDays(-4), PharmacistName = "Rahul Verma" },
                new PharmacyRecord { PatientId = p2.PatientId, PatientName = p2.Name, TreatmentPlanId = tp2.TreatmentPlanId, MedicineName = "Azithromycin 500mg", Quantity = 5, Status = "PENDING" }
            };
            context.PharmacyRecords.AddRange(pharmacyRecords);

            var billingRecords = new[]
            {
                new BillingRecord { PatientId = p1.PatientId, PatientName = p1.Name, ConsultationFee = 500, LabCharges = 1500, MedicineCharges = 450, RoomCharges = 3000, Status = "PAID", CreatedDate = DateTime.Now.AddDays(-1), PaymentDate = DateTime.Now.AddDays(-1), IsDischarged = true, DischargeDate = DateTime.Now.AddDays(-1), DischargeRemarks = "Recovered fully." },
                new BillingRecord { PatientId = p2.PatientId, PatientName = p2.Name, ConsultationFee = 500, LabCharges = 1200, MedicineCharges = 250, RoomCharges = 6000, Status = "PENDING", CreatedDate = DateTime.Now }
            };
            context.BillingRecords.AddRange(billingRecords);

            var dischargeRecords = new[]
            {
                new DischargeRecord { PatientId = p1.PatientId, PatientName = p1.Name, AdmissionDate = DateTime.Now.AddDays(-5), DischargeDate = DateTime.Now.AddDays(-1), Remarks = "Recovered fully.", Summary = "Successful laparoscopic appendectomy." }
            };
            context.DischargeRecords.AddRange(dischargeRecords);

            var medicineStocks = new[]
            {
                new MedicineStock { MedicineName = "Amoxicillin 500mg", Quantity = 120 },
                new MedicineStock { MedicineName = "Paracetamol 650mg", Quantity = 350 },
                new MedicineStock { MedicineName = "Azithromycin 500mg", Quantity = 80 },
                new MedicineStock { MedicineName = "Pantoprazole 40mg", Quantity = 200 }
            };
            context.MedicineStocks.AddRange(medicineStocks);

            context.SaveChanges();
        }
    }
}
