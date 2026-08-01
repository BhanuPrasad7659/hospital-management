using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    [Authorize(Roles = "admin,doctor")]
    public class DoctorApiController : ControllerBase
    {
        private readonly ITreatmentPlanService _treatmentPlanService;
        private readonly IPatientService _patientService;
        private readonly IEhrService _ehrService;
        private readonly IOrderTestLabService _orderTestLabService;

        public DoctorApiController(
            ITreatmentPlanService treatmentPlanService, 
            IPatientService patientService,
            IEhrService ehrService,
            IOrderTestLabService orderTestLabService)
        {
            _treatmentPlanService = treatmentPlanService;
            _patientService = patientService;
            _ehrService = ehrService;
            _orderTestLabService = orderTestLabService;
        }

        [HttpGet("treatments")]
        public IActionResult GetTreatments() => Ok(_treatmentPlanService.GetTreatments());

        [HttpGet("treatments/{id:int}")]
        public IActionResult GetTreatment(int id)
        {
            var tx = _treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id);
            return tx == null ? NotFound($"Treatment plan #{id} not found.") : Ok(tx);
        }

        [HttpGet("treatments/patient/{patientId:int}")]
        public IActionResult GetTreatmentsForPatient(int patientId) => Ok(_treatmentPlanService.GetTreatmentsForPatient(patientId));

        [HttpPost("treatments")]
        public IActionResult CreateTreatment([FromBody] TreatmentPlan plan)
        {
            if (plan == null || plan.PatientId <= 0 || string.IsNullOrEmpty(plan.TreatmentDescription))
                return BadRequest("PatientId and TreatmentDescription are required.");

            if (_patientService.GetPatient(plan.PatientId) == null)
                return NotFound($"Patient #{plan.PatientId} not found.");

            try
            {
                var created = _treatmentPlanService.CreateTreatmentPlan(plan);
                return CreatedAtAction(nameof(GetTreatment), new { id = created.TreatmentPlanId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("treatments/{id:int}")]
        public IActionResult UpdateTreatment(int id, [FromBody] TreatmentPlan planDetails)
        {
            if (_treatmentPlanService.GetTreatments().All(t => t.TreatmentPlanId != id))
                return NotFound($"Treatment plan #{id} not found.");

            planDetails.TreatmentPlanId = id;
            _treatmentPlanService.UpdateTreatmentPlan(planDetails);
            return Ok(_treatmentPlanService.GetTreatments().FirstOrDefault(t => t.TreatmentPlanId == id));
        }

        [HttpDelete("treatments/{id:int}")]
        public IActionResult DeleteTreatment(int id)
        {
            _treatmentPlanService.DeleteTreatmentPlan(id);
            return NoContent();
        }

        [HttpGet("ehr")]
        public IActionResult GetEhrRecords() => Ok(_ehrService.GetEhrRecords());

        [HttpGet("ehr/patient/{patientId:int}")]
        public IActionResult GetEhrForPatient(int patientId) => Ok(_ehrService.GetEhrForPatient(patientId));

        [HttpPost("ehr")]
        public IActionResult CreateEhr([FromBody] EhrRecord record)
        {
            if (record == null || record.PatientId <= 0 || string.IsNullOrEmpty(record.Diagnosis))
                return BadRequest("PatientId and Diagnosis are required.");

            if (_patientService.GetPatient(record.PatientId) == null)
                return NotFound($"Patient #{record.PatientId} not found.");

            record.VisitDate = DateTime.Now;
            return Ok(_ehrService.CreateEhr(record));
        }

        [HttpPost("lab-orders")]
        public IActionResult OrderLabTests([FromBody] ApiOrderLabRequest request)
        {
            if (request == null || request.PatientId <= 0 || request.SelectedTests == null || !request.SelectedTests.Any())
                return BadRequest("PatientId and SelectedTests (list) are required.");

            var patient = _patientService.GetPatient(request.PatientId);
            if (patient == null) 
                return NotFound($"Patient #{request.PatientId} not found.");

            var createdOrders = request.SelectedTests.Select(test => _orderTestLabService.CreateOrderTestLab(new OrderTestLab
            {
                PatientId = request.PatientId,
                PatientName = patient.Name,
                DoctorId = request.DoctorId,
                DoctorName = request.DoctorName,
                TestName = test,
                Status = "ORDERED",
                OrderDate = DateTime.Now
            })).ToList();

            return Ok(createdOrders);
        }

        public class ApiOrderLabRequest
        {
            public int PatientId { get; set; }
            public int? DoctorId { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public List<string> SelectedTests { get; set; } = new List<string>();
        }
    }
}
