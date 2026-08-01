using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface ITreatmentPlanRepository : IRepository<TreatmentPlan>
    {
        List<TreatmentPlan> GetTreatmentsForPatient(int patientId);
        TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan);
        void UpdateTreatmentPlan(TreatmentPlan plan);
        void DeleteTreatmentPlan(int planId);
    }
}
