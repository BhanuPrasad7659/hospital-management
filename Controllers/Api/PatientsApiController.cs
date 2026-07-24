using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/patients")]
    public class PatientsApiController : ControllerBase
    {
                private readonly IPatientService _patientService;

        public PatientsApiController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: api/patients
        [HttpGet]
        public IActionResult GetPatients()
        {
            var patients = _patientService.GetPatients();
            var dtos = patients.Select(p => p.ToDto());
            return Ok(dtos);
        }

        // GET: api/patients/{id}
        [HttpGet("{id}")]
        public IActionResult GetPatient(int id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");
            return Ok(patient.ToDto());
        }

        // POST: api/patients
        [HttpPost]
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

        // PUT: api/patients/{id}
        [HttpPut("{id}")]
        public IActionResult UpdatePatient(int id, [FromBody] PatientUpdateDto requestDto)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");

            requestDto.UpdateEntity(patient);
            _patientService.UpdatePatient(patient);
            
            var updated = _patientService.GetPatient(id);
            return Ok(updated?.ToDto());
        }

        // DELETE: api/patients/{id}
        [HttpDelete("{id}")]
        public IActionResult DeletePatient(int id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null) return NotFound($"Patient with ID #{id} not found.");

            _patientService.DeletePatient(id);
            return NoContent();
        }
    }
}
