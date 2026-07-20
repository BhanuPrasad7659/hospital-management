using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/billing")]
    public class BillingApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public BillingApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/billing
        [HttpGet]
        public IActionResult GetBillingRecords()
        {
            var bills = _hospitalService.GetBillingRecords();
            return Ok(bills);
        }

        // GET: api/billing/{id}
        [HttpGet("{id}")]
        public IActionResult GetBillingRecord(string id)
        {
            var bill = _hospitalService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record '{id}' not found.");
            return Ok(bill);
        }

        // GET: api/billing/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public IActionResult GetBillingForPatient(string patientId)
        {
            var bill = _hospitalService.GetBillingForPatient(patientId);
            if (bill == null) return NotFound($"Billing record for Patient '{patientId}' not found.");
            return Ok(bill);
        }

        // POST: api/billing
        [HttpPost]
        public IActionResult GenerateBill([FromBody] BillGenerationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PatientId))
            {
                return BadRequest("PatientId is required.");
            }

            var patient = _hospitalService.GetPatient(request.PatientId);
            if (patient == null) return NotFound($"Patient '{request.PatientId}' not found.");

            var bill = _hospitalService.GenerateBill(
                request.PatientId, 
                request.ConsultationFee, 
                request.LabCharges, 
                request.MedicineCharges, 
                request.RoomCharges
            );
            return CreatedAtAction(nameof(GetBillingRecord), new { id = bill.BillingRecordId }, bill);
        }

        // PUT: api/billing/pay/{id}
        [HttpPut("pay/{id}")]
        public IActionResult ProcessPayment(string id)
        {
            var bill = _hospitalService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record '{id}' not found.");

            if (bill.Status == "PAID")
            {
                return BadRequest("Bill already paid.");
            }

            _hospitalService.ProcessPayment(id);
            var updated = _hospitalService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            return Ok(updated);
        }

        // PUT: api/billing/discharge/{patientId}
        [HttpPut("discharge/{patientId}")]
        public IActionResult DischargePatient(string patientId, [FromQuery] string remarks)
        {
            var patient = _hospitalService.GetPatient(patientId);
            if (patient == null) return NotFound($"Patient '{patientId}' not found.");

            var bill = _hospitalService.GetBillingForPatient(patientId);
            if (bill != null && bill.Status == "PENDING")
            {
                return BadRequest("Cannot discharge patient with a pending bill. Settlement is required.");
            }

            _hospitalService.DischargePatient(patientId, remarks ?? "Safe to discharge.");
            return Ok(new { Message = $"Patient '{patientId}' successfully discharged." });
        }

        // DELETE: api/billing/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteBillingRecord(string id)
        {
            var bill = _hospitalService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record '{id}' not found.");

            _hospitalService.DeleteBillingRecord(id);
            return NoContent();
        }

        public class BillGenerationRequest
        {
            public string PatientId { get; set; } = string.Empty;
            public decimal ConsultationFee { get; set; }
            public decimal LabCharges { get; set; }
            public decimal MedicineCharges { get; set; }
            public decimal RoomCharges { get; set; }
        }
    }
}
