using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/billing")]
    public class BillingApiController : ControllerBase
    {
                private readonly IDischargeService _dischargeService;
        private readonly IBillingService _billingService;
        private readonly IPatientService _patientService;

        public BillingApiController(IDischargeService dischargeService, IBillingService billingService, IPatientService patientService)
        {
            _dischargeService = dischargeService;
            _billingService = billingService;
            _patientService = patientService;
        }

        // GET: api/billing
        [HttpGet]
        public IActionResult GetBillingRecords()
        {
            var bills = _billingService.GetBillingRecords();
            return Ok(bills);
        }

        // GET: api/billing/{id}
        [HttpGet("{id}")]
        public IActionResult GetBillingRecord(int id)
        {
            var bill = _billingService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record #{id} not found.");
            return Ok(bill);
        }

        // GET: api/billing/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public IActionResult GetBillingForPatient(int patientId)
        {
            var bill = _billingService.GetBillingForPatient(patientId);
            if (bill == null) return NotFound($"Billing record for Patient #{patientId} not found.");
            return Ok(bill);
        }

        // POST: api/billing
        [HttpPost]
        public IActionResult GenerateBill([FromBody] BillGenerationRequest request)
        {
            if (request == null || request.PatientId <= 0)
            {
                return BadRequest("Valid PatientId is required.");
            }

            var patient = _patientService.GetPatient(request.PatientId);
            if (patient == null) return NotFound($"Patient #{request.PatientId} not found.");

            var bill = _billingService.GenerateBill(
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
        public IActionResult ProcessPayment(int id)
        {
            var bill = _billingService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record #{id} not found.");

            if (bill.Status == "PAID")
            {
                return BadRequest("Bill already paid.");
            }

            _billingService.ProcessPayment(id);
            var updated = _billingService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            return Ok(updated);
        }

        // PUT: api/billing/discharge/{patientId}
        [HttpPut("discharge/{patientId}")]
        public IActionResult DischargePatient(int patientId, [FromQuery] string remarks)
        {
            var patient = _patientService.GetPatient(patientId);
            if (patient == null) return NotFound($"Patient #{patientId} not found.");

            var bill = _billingService.GetBillingForPatient(patientId);
            if (bill != null && bill.Status == "PENDING")
            {
                return BadRequest("Cannot discharge patient with a pending bill. Settlement is required.");
            }

            _dischargeService.DischargePatient(patientId, remarks ?? "Safe to discharge.");
            return Ok(new { Message = $"Patient #{patientId} successfully discharged." });
        }

        // DELETE: api/billing/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteBillingRecord(int id)
        {
            var bill = _billingService.GetBillingRecords().FirstOrDefault(b => b.BillingRecordId == id);
            if (bill == null) return NotFound($"Billing record #{id} not found.");

            _billingService.DeleteBillingRecord(id);
            return NoContent();
        }

        public class BillGenerationRequest
        {
            public int PatientId { get; set; }
            public decimal ConsultationFee { get; set; }
            public decimal LabCharges { get; set; }
            public decimal MedicineCharges { get; set; }
            public decimal RoomCharges { get; set; }
        }
    }
}
