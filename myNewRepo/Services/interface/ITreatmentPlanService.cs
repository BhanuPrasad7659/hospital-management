using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface ITreatmentPlanService
    {
        List<TreatmentPlan> GetTreatments();
        List<TreatmentPlan> GetTreatmentsForPatient(int patientId);
        TreatmentPlan CreateTreatmentPlan(TreatmentPlan plan);
        void DeleteTreatmentPlan(int planId);
        void UpdateTreatmentPlan(TreatmentPlan plan);
    }
}
