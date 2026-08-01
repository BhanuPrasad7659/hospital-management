using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/pharmacist")]
    [Authorize]
    public class PharmacistApiController : ControllerBase
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly IMedicineStockService _medicineStockService;

        public PharmacistApiController(IPharmacyService pharmacyService, IMedicineStockService medicineStockService)
        {
            _pharmacyService = pharmacyService;
            _medicineStockService = medicineStockService;
        }

        [HttpGet("records")]
        public IActionResult GetPharmacyRecords()
        {
            var records = _pharmacyService.GetPharmacyRecords();
            return Ok(records);
        }

        [HttpGet("records/{id:int}")]
        public IActionResult GetPharmacyRecord(int id)
        {
            var record = _pharmacyService.GetPharmacyRecords().FirstOrDefault(r => r.PharmacyRecordId == id);
            if (record == null) return NotFound($"Pharmacy record #{id} not found.");
            return Ok(record);
        }

        [HttpGet("stock")]
        public IActionResult GetStock()
        {
            var stock = _medicineStockService.MedicineStock;
            return Ok(stock);
        }

        [HttpPost("stock")]
        [Authorize(Roles = "admin,pharmacist,pharmiacist")]
        public IActionResult UpdateStock([FromBody] StockUpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.MedicineName))
            {
                return BadRequest("MedicineName is required.");
            }

            _medicineStockService.UpdateMedicineStock(request.MedicineName, request.Amount);
            return Ok(new { MedicineName = request.MedicineName, NewStock = _medicineStockService.MedicineStock.GetValueOrDefault(request.MedicineName, 0) });
        }

        [HttpPut("dispense/{id:int}")]
        [Authorize(Roles = "admin,pharmacist,pharmiacist")]
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

        [HttpGet("prices")]
        public IActionResult GetPrices()
        {
            var prices = _medicineStockService.MedicinePrices;
            return Ok(prices);
        }

        [HttpDelete("records/{id:int}")]
        [Authorize(Roles = "admin,pharmacist,pharmiacist")]
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
