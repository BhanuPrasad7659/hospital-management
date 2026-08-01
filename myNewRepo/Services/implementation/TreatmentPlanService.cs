using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class TreatmentPlanService : ITreatmentPlanService
    {
        private readonly ITreatmentPlanRepository _treatmentRepository;
        private readonly IOrderTestLabRepository _orderTestLabRepository;

        public TreatmentPlanService(
            ITreatmentPlanRepository treatmentRepository,
            IOrderTestLabRepository orderTestLabRepository)
        {
            _treatmentRepository = treatmentRepository;
            _orderTestLabRepository = orderTestLabRepository;
        }

        public List<TreatmentPlan> GetTreatments() => (List<TreatmentPlan>)_treatmentRepository.GetAll();

        public List<TreatmentPlan> GetTreatmentsForPatient(int patientId) => _treatmentRepository.GetTreatmentsForPatient(patientId);

        public TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan)
        {
            var hasCompletedLab = _orderTestLabRepository.GetOrderTestLabsForPatient(plan.PatientId)
                .Any(l => l.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase));

            if (!hasCompletedLab)
            {
                throw new InvalidOperationException("Cannot start treatment. You must wait for the lab technician to upload the test results first.");
            }

            plan.Medication = plan.Medication ?? "";
            plan.Duration = plan.Duration ?? "";
            plan.Instructions = plan.Instructions ?? "";

            return _treatmentRepository.CreateTreatmentPlan(plan);
        }

        public void DeleteTreatmentPlan(int planId) => _treatmentRepository.DeleteTreatmentPlan(planId);

        public void UpdateTreatmentPlan(TreatmentPlan plan)
        {
            plan.Medication = plan.Medication ?? "";
            plan.Duration = plan.Duration ?? "";
            plan.Instructions = plan.Instructions ?? "";

            _treatmentRepository.UpdateTreatmentPlan(plan);
        }
    }
}
