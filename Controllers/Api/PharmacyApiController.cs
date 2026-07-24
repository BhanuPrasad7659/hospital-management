using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/pharmacy")]
    public class PharmacyApiController : ControllerBase
    {
                private readonly IPharmacyService _pharmacyService;
        private readonly IMedicineStockService _medicineStockService;

        public PharmacyApiController(IPharmacyService pharmacyService, IMedicineStockService medicineStockService)
        {
            _pharmacyService = pharmacyService;
            _medicineStockService = medicineStockService;
        }

        // GET: api/pharmacy
        [HttpGet]
        public IActionResult GetPharmacyRecords()
        {
            var records = _pharmacyService.GetPharmacyRecords();
            return Ok(records);
        }

        // GET: api/pharmacy/{id}
        [HttpGet("{id}")]
        public IActionResult GetPharmacyRecord(int id)
        {
            var record = _pharmacyService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record #{id} not found.");
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

            _medicineStockService.UpdateMedicineStock(request.MedicineName, request.Amount);
            return Ok(new { MedicineName = request.MedicineName, NewStock = _medicineStockService.MedicineStock.GetValueOrDefault(request.MedicineName, 0) });
        }

        // PUT: api/pharmacy/dispense/{id}
        [HttpPut("dispense/{id}")]
        public IActionResult Dispense(int id, [FromQuery] string pharmacistName)
        {
            var record = _pharmacyService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record #{id} not found.");

            if (record.Status == "DISPENSED")
            {
                return BadRequest("Medicine already dispensed.");
            }

            if (_medicineStockService.MedicineStock.TryGetValue(record.MedicineName, out int stock) && stock < record.Quantity)
            {
                return BadRequest($"Insufficient stock for {record.MedicineName}. Available: {stock}. Required: {record.Quantity}.");
            }

            _pharmacyService.DispenseMedicine(id, pharmacistName ?? "Rahul Verma");
            var updated = _pharmacyService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            return Ok(updated);
        }

        // DELETE: api/pharmacy/{id}
        [HttpDelete("{id}")]
        public IActionResult DeletePharmacyRecord(int id)
        {
            var record = _pharmacyService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record #{id} not found.");

            _pharmacyService.DeletePharmacyRecord(id);
            return NoContent();
        }

        public class StockUpdateRequest
        {
            public string MedicineName { get; set; } = string.Empty;
            public int Amount { get; set; }
        }
    }
}
