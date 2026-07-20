using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/treatments")]
    public class TreatmentsApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public TreatmentsApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/treatments
        [HttpGet]
        public IActionResult GetTreatments()
        {
            var treatments = _hospitalService.GetTreatments();
            return Ok(treatments);
        }

        // GET: api/treatments/{id}
        [HttpGet("{id}")]
        public IActionResult GetTreatment(string id)
        {
            var treatment = _hospitalService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan '{id}' not found.");
            return Ok(treatment);
        }

        // GET: api/treatments/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public IActionResult GetTreatmentsForPatient(string patientId)
        {
            var treatments = _hospitalService.GetTreatmentsForPatient(patientId);
            return Ok(treatments);
        }

        // POST: api/treatments
        [HttpPost]
        public IActionResult CreateTreatment([FromBody] TreatmentPlan plan)
        {
            if (plan == null || string.IsNullOrEmpty(plan.PatientId) || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                return BadRequest("PatientId and TreatmentDescription are required.");
            }

            var patient = _hospitalService.GetPatient(plan.PatientId);
            if (patient == null) return NotFound($"Patient '{plan.PatientId}' not found.");

            var created = _hospitalService.CreateTreatmentPlan(plan);
            return CreatedAtAction(nameof(GetTreatment), new { id = created.TreatmentPlanId }, created);
        }

        // PUT: api/treatments/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTreatment(string id, [FromBody] TreatmentPlan planDetails)
        {
            var treatment = _hospitalService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan '{id}' not found.");

            planDetails.TreatmentPlanId = id;
            _hospitalService.UpdateTreatmentPlan(planDetails);
            var updated = _hospitalService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            return Ok(updated);
        }

        // DELETE: api/treatments/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTreatment(string id)
        {
            var treatment = _hospitalService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            if (treatment == null) return NotFound($"Treatment plan '{id}' not found.");

            _hospitalService.DeleteTreatmentPlan(id);
            return NoContent();
        }
    }
}
