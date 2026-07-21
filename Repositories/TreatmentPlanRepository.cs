using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
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
            plan.PrescribedDate = DateTime.Now;
            Add(plan);
            SaveChanges();

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
                existing.Diagnosis = plan.Diagnosis;
                existing.TreatmentDescription = plan.TreatmentDescription;
                existing.Medication = plan.Medication;
                existing.Duration = plan.Duration;
                existing.Instructions = plan.Instructions;
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
