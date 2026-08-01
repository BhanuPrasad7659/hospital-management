using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/receptionist")]
    [Authorize]
    public class ReceptionistApiController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAdmissionService _admissionService;

        public ReceptionistApiController(IPatientService patientService, IAdmissionService admissionService)
        {
            _patientService = patientService;
            _admissionService = admissionService;
        }

        [HttpGet("patients")]
        public IActionResult GetPatients()
        {
            var patients = _patientService.GetPatients();
            var dtos = patients.Select(p => p.ToDto());
            return Ok(dtos);
        }

        [HttpGet("patients/{id:int}")]
        public IActionResult GetPatient(int id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");
            return Ok(patient.ToDto());
        }

        [HttpPost("patients")]
        [Authorize(Roles = "admin,receptionist")]
        public IActionResult RegisterPatient([FromBody] PatientCreateDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request body is missing.");
            }

            var patientEntity = requestDto.ToEntity();
            var registered = _patientService.RegisterPatient(patientEntity);
            return CreatedAtAction(nameof(GetPatient), new { id = registered.PatientId }, registered.ToDto());
        }

        [HttpPut("patients/{id:int}")]
        [Authorize(Roles = "admin,receptionist")]
        public IActionResult UpdatePatient(int id, [FromBody] PatientUpdateDto requestDto)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");

            requestDto.UpdateEntity(patient);
            _patientService.UpdatePatient(patient);
            
            var updated = _patientService.GetPatient(id);
            return Ok(updated?.ToDto());
        }

        [HttpDelete("patients/{id:int}")]
        [Authorize(Roles = "admin,receptionist")]
        public IActionResult DeletePatient(int id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");

            _patientService.DeletePatient(id);
            return NoContent();
        }

        [HttpGet("admissions")]
        public IActionResult GetAdmissions()
        {
            var admissions = _admissionService.GetAdmissions();
            return Ok(admissions);
        }

        [HttpGet("admissions/{id:int}")]
        public IActionResult GetAdmission(int id)
        {
            var admission = _admissionService.GetAdmission(id);
            if (admission == null) return NotFound($"Admission record #{id} not found.");
            return Ok(admission);
        }

        [HttpPost("admissions")]
        [Authorize(Roles = "admin,receptionist")]
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

        [HttpPut("admissions/{id:int}")]
        [Authorize(Roles = "admin,receptionist")]
        public IActionResult UpdateAdmission(int id, [FromBody] Admission admissionDetails)
        {
            var admission = _admissionService.GetAdmission(id);
            if (admission == null) return NotFound($"Admission record #{id} not found.");

            admissionDetails.AdmissionId = id;
            _admissionService.UpdateAdmission(admissionDetails);
            var updated = _admissionService.GetAdmission(id);
            return Ok(updated);
        }

        [HttpDelete("admissions/{id:int}")]
        [Authorize(Roles = "admin,receptionist")]
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
