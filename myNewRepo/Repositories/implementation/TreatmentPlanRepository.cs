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
            var patient = _context.Patients.FirstOrDefault(p => p.PatientId == plan.PatientId);
            var patientName = patient?.Name ?? "Unknown";

            if (string.IsNullOrWhiteSpace(plan.PatientName))
            {
                plan.PatientName = patientName;
            }

            if (plan.DoctorId.HasValue && plan.DoctorId.Value > 0 && string.IsNullOrWhiteSpace(plan.DoctorName))
            {
                var doctor = _context.Users.FirstOrDefault(u => u.Id == plan.DoctorId.Value);
                if (doctor != null)
                {
                    plan.DoctorName = doctor.FullName;
                }
            }

            plan.Medication = plan.Medication ?? "";
            plan.Duration = plan.Duration ?? "";
            plan.Instructions = plan.Instructions ?? "";

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
                        PatientName = patientName,
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

                var patient = _context.Patients.FirstOrDefault(p => p.PatientId == existing.PatientId);
                if (patient != null) existing.PatientName = patient.Name;

                existing.Diagnosis = plan.Diagnosis;
                existing.TreatmentDescription = plan.TreatmentDescription;

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
