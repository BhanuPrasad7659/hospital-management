using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class TreatmentPlanRepository : Repository<TreatmentPlan>, ITreatmentPlanRepository
    {
        private readonly HospitalDbContext _context;

        public TreatmentPlanRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public List<TreatmentPlan> GetTreatmentsForPatient(int patientId)
        {
            return Find(t => t.PatientId == patientId).ToList();
        }

        public TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan)
        {
            if (plan.DoctorId.HasValue && plan.DoctorId.Value > 0 && string.IsNullOrWhiteSpace(plan.DoctorName))
            {
                var doctor = _context.Users.FirstOrDefault(u => u.Id == plan.DoctorId.Value);
                if (doctor != null)
                {
                    plan.DoctorName = doctor.FullName;
                }
            }

            // --- SQL EXCEPTION PREVENTION ---
            // Ensure none of these fields hit the database as NULL
            plan.Medication = plan.Medication ?? "";
            plan.Duration = plan.Duration ?? "";
            plan.Instructions = plan.Instructions ?? "";
            // --------------------------------

            plan.PrescribedDate = DateTime.Now;
            Add(plan);
            SaveChanges();

            // Creates pharmacy records for prescribed medicines
            if (!string.IsNullOrWhiteSpace(plan.Medication))
            {
                var medicines = plan.Medication.Split(',');
                foreach (var med in medicines)
                {
                    var medClean = med.Trim();
                    if (string.IsNullOrEmpty(medClean)) continue;

                    _context.PharmacyRecords.Add(new PharmacyRecord
                    {
                        PatientId = plan.PatientId,
                        TreatmentPlanId = plan.TreatmentPlanId,
                        MedicineName = medClean,
                        Quantity = 10,
                        Status = "PENDING"
                    });
                }
                SaveChanges();
            }

            return plan;
        }

        public void UpdateTreatmentPlan(TreatmentPlan plan)
        {
            var existing = GetById(plan.TreatmentPlanId);
            if (existing != null)
            {
                if (plan.DoctorId.HasValue && plan.DoctorId.Value > 0)
                {
                    existing.DoctorId = plan.DoctorId;
                    var doctor = _context.Users.FirstOrDefault(u => u.Id == plan.DoctorId.Value);
                    if (doctor != null) existing.DoctorName = doctor.FullName;
                }

                existing.Diagnosis = plan.Diagnosis;
                existing.TreatmentDescription = plan.TreatmentDescription;

                // Ensure updates also don't push NULL to the database
                existing.Medication = plan.Medication ?? existing.Medication ?? "";
                existing.Duration = plan.Duration ?? existing.Duration ?? "";
                existing.Instructions = plan.Instructions ?? existing.Instructions ?? "";

                Update(existing);
                SaveChanges();
            }
        }

        public void DeleteTreatmentPlan(int planId)
        {
            var plan = GetById(planId);
            if (plan != null)
            {
                Delete(plan);
                SaveChanges();
            }
        }
    }
}