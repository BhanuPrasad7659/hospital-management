using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/admissions")]
    public class AdmissionsApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public AdmissionsApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/admissions
        [HttpGet]
        public IActionResult GetAdmissions()
        {
            var admissions = _hospitalService.GetAdmissions();
            return Ok(admissions);
        }

        // GET: api/admissions/{id}
        [HttpGet("{id}")]
        public IActionResult GetAdmission(string id)
        {
            var admission = _hospitalService.GetAdmissions().FirstOrDefault(a => a.AdmissionId == id);
            if (admission == null) return NotFound($"Admission record '{id}' not found.");
            return Ok(admission);
        }

        // POST: api/admissions
        [HttpPost]
        public IActionResult AdmitPatient([FromBody] Admission admissionRequest)
        {
            if (admissionRequest == null || string.IsNullOrEmpty(admissionRequest.PatientId) || string.IsNullOrEmpty(admissionRequest.Ward) || string.IsNullOrEmpty(admissionRequest.BedNumber) || string.IsNullOrEmpty(admissionRequest.AssignedDoctorUsername))
            {
                return BadRequest("PatientId, Ward, BedNumber, and AssignedDoctorUsername are required.");
            }

            var patient = _hospitalService.GetPatient(admissionRequest.PatientId);
            if (patient == null) return NotFound($"Patient '{admissionRequest.PatientId}' not found.");

            var created = _hospitalService.AdmitPatient(
                admissionRequest.PatientId, 
                admissionRequest.Ward, 
                admissionRequest.BedNumber, 
                admissionRequest.AssignedDoctorUsername
            );
            return CreatedAtAction(nameof(GetAdmission), new { id = created.AdmissionId }, created);
        }

        // PUT: api/admissions/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAdmission(string id, [FromBody] Admission admissionDetails)
        {
            var admission = _hospitalService.GetAdmissions().FirstOrDefault(a => a.AdmissionId == id);
            if (admission == null) return NotFound($"Admission record '{id}' not found.");

            admissionDetails.AdmissionId = id;
            _hospitalService.UpdateAdmission(admissionDetails);
            var updated = _hospitalService.GetAdmissions().FirstOrDefault(a => a.AdmissionId == id);
            return Ok(updated);
        }

        // DELETE: api/admissions/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAdmission(string id)
        {
            var admission = _hospitalService.GetAdmissions().FirstOrDefault(a => a.AdmissionId == id);
            if (admission == null) return NotFound($"Admission record '{id}' not found.");

            _hospitalService.DeleteAdmission(id);
            return NoContent();
        }
    }
}
