using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;
using System;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/treatments")]
    public class TreatmentsApiController : ControllerBase
    {
                private readonly ITreatmentPlanService _treatmentPlanService;
        private readonly IPatientService _patientService;

        public TreatmentsApiController(ITreatmentPlanService treatmentPlanService, IPatientService patientService)
        {
            _treatmentPlanService = treatmentPlanService;
            _patientService = patientService;
        }

        [HttpGet]
        public IActionResult GetTreatments()
        {
            var treatments = _treatmentPlanService.GetTreatments();
            return Ok(treatments);
        }

        [HttpGet("{id}")]
        public IActionResult GetTreatment(int id)
        {
            var treatment = _treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan #{id} not found.");
            return Ok(treatment);
        }

        [HttpGet("patient/{patientId}")]
        public IActionResult GetTreatmentsForPatient(int patientId)
        {
            var treatments = _treatmentPlanService.GetTreatmentsForPatient(patientId);
            return Ok(treatments);
        }

        [HttpPost]
        public IActionResult CreateTreatment([FromBody] TreatmentPlan plan)
        {
            if (plan == null || plan.PatientId <= 0 || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                return BadRequest("PatientId and TreatmentDescription are required.");
            }

            var patient = _patientService.GetPatient(plan.PatientId);
            if (patient == null) return NotFound($"Patient #{plan.PatientId} not found.");

            try
            {
                // NEW: Will catch the exception if lab test is not completed
                var created = _treatmentPlanService.CreateTreatmentPlan(plan);
                return CreatedAtAction(nameof(GetTreatment), new { id = created.TreatmentPlanId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTreatment(int id, [FromBody] TreatmentPlan planDetails)
        {
            var treatment = _treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan #{id} not found.");

            planDetails.TreatmentPlanId = id;
            _treatmentPlanService.UpdateTreatmentPlan(planDetails);
            var updated = _treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTreatment(int id)
        {
            var treatment = _treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan #{id} not found.");

            _treatmentPlanService.DeleteTreatmentPlan(id);
            return NoContent();
        }
    }
}