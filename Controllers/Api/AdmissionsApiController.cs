using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/admissions")]
    public class AdmissionsApiController : ControllerBase
    {
                private readonly IAdmissionService _admissionService;
        private readonly IPatientService _patientService;

        public AdmissionsApiController(IAdmissionService admissionService, IPatientService patientService)
        {
            _admissionService = admissionService;
            _patientService = patientService;
        }

        // GET: api/admissions
        [HttpGet]
        public IActionResult GetAdmissions()
        {
            var admissions = _admissionService.GetAdmissions();
            return Ok(admissions);
        }

        // GET: api/admissions/{id}
        [HttpGet("{id}")]
        public IActionResult GetAdmission(int id)
        {
            var admission = _admissionService.GetAdmission(id);
            if (admission == null) return NotFound($"Admission record #{id} not found.");
            return Ok(admission);
        }

        // POST: api/admissions
        [HttpPost("post")]
        public IActionResult AdmitPatient([FromBody] ApiAdmissionRequest admissionRequest)
        {
            if (admissionRequest == null || admissionRequest.PatientId <= 0 || string.IsNullOrEmpty(admissionRequest.Ward) || string.IsNullOrEmpty(admissionRequest.BedNumber) || admissionRequest.AssignedDoctorId <= 0)
            {
                return BadRequest("PatientId, Ward, BedNumber, and AssignedDoctorId are required.");
            }

            var patient = _patientService.GetPatient(admissionRequest.PatientId);
            if (patient == null) return NotFound($"Patient #{admissionRequest.PatientId} not found.");

            var created = _admissionService.AdmitPatient(
                admissionRequest.PatientId, 
                admissionRequest.Ward, 
                admissionRequest.BedNumber, 
                admissionRequest.AssignedDoctorId
            );
            return CreatedAtAction(nameof(GetAdmission), new { id = created.AdmissionId }, created);
        }

        // PUT: api/admissions/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAdmission(int id, [FromBody] Admission admissionDetails)
        {
            var admission = _admissionService.GetAdmission(id);
            if (admission == null) return NotFound($"Admission record #{id} not found.");

            admissionDetails.AdmissionId = id;
            _admissionService.UpdateAdmission(admissionDetails);
            var updated = _admissionService.GetAdmission(id);
            return Ok(updated);
        }

        // DELETE: api/admissions/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAdmission(int id)
        {
            var admission = _admissionService.GetAdmission(id);
            if (admission == null) return NotFound($"Admission record #{id} not found.");

            _admissionService.DeleteAdmission(id);
            return NoContent();
        }

        public class ApiAdmissionRequest
        {
            public int PatientId { get; set; }
            public string Ward { get; set; } = string.Empty;
            public string BedNumber { get; set; } = string.Empty;
            public int AssignedDoctorId { get; set; }
        }
    }
}
