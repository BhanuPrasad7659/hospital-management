using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/pharmacy")]
    public class PharmacyApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public PharmacyApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/pharmacy
        [HttpGet]
        public IActionResult GetPharmacyRecords()
        {
            var records = _hospitalService.GetPharmacyRecords();
            return Ok(records);
        }

        // GET: api/pharmacy/{id}
        [HttpGet("{id}")]
        public IActionResult GetPharmacyRecord(string id)
        {
            var record = _hospitalService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record '{id}' not found.");
            return Ok(record);
        }

        // POST: api/pharmacy/stock
        [HttpPost("stock")]
        public IActionResult UpdateStock([FromBody] StockUpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.MedicineName))
            {
                return BadRequest("MedicineName is required.");
            }

            _hospitalService.UpdateMedicineStock(request.MedicineName, request.Amount);
            return Ok(new { MedicineName = request.MedicineName, NewStock = _hospitalService.MedicineStock.GetValueOrDefault(request.MedicineName, 0) });
        }

        // PUT: api/pharmacy/dispense/{id}
        [HttpPut("dispense/{id}")]
        public IActionResult Dispense(string id, [FromQuery] string pharmacistName)
        {
            var record = _hospitalService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record '{id}' not found.");

            if (record.Status == "DISPENSED")
            {
                return BadRequest("Medicine already dispensed.");
            }

            // Check stock level
            if (_hospitalService.MedicineStock.TryGetValue(record.MedicineName, out int stock) && stock < record.Quantity)
            {
                return BadRequest($"Insufficient stock for {record.MedicineName}. Available: {stock}. Required: {record.Quantity}.");
            }

            _hospitalService.DispenseMedicine(id, pharmacistName ?? "Rahul Verma");
            var updated = _hospitalService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            return Ok(updated);
        }

        // DELETE: api/pharmacy/{id}
        [HttpDelete("{id}")]
        public IActionResult DeletePharmacyRecord(string id)
        {
            var record = _hospitalService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record '{id}' not found.");

            _hospitalService.DeletePharmacyRecord(id);
            return NoContent();
        }

        public class StockUpdateRequest
        {
            public string MedicineName { get; set; } = string.Empty;
            public int Amount { get; set; }
        }
    }
}
