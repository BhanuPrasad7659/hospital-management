using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,pharmacist,pharmiacist")]
    public class PharmacistController : Controller
    {
                private readonly IPatientService _patientService;
        private readonly IPharmacyService _pharmacyService;
        private readonly IMedicineStockService _medicineStockService;

        public PharmacistController(IPatientService patientService, IPharmacyService pharmacyService, IMedicineStockService medicineStockService)
        {
            _patientService = patientService;
            _pharmacyService = pharmacyService;
            _medicineStockService = medicineStockService;
        }

        [Route("pharmacist/dashboard")]
        public IActionResult Dashboard()
        {
            var records = _pharmacyService.GetPharmacyRecords();
            var pending = records.Count(r => r.Status == "PENDING");
            var dispensed = records.Count(r => r.Status == "DISPENSED");
            var totalStock = _medicineStockService.MedicineStock.Values.Sum();

            var viewModel = new DashboardViewModel
            {
                TotalPatients = totalStock,
                ActiveCases = dispensed,
                ActiveTreatments = pending,
                RecentActivities = new List<string>
                {
                    "Dispensed Amoxicillin 500mg (14 units) to Patient Amit Sharma.",
                    "Restocked Metformin 500mg (+100 units).",
                    "Received new prescription order from Dr. Ramesh."
                },
                RoleName = "Pharmacy Dispensary",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Pharmacist"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("pharmacist/dispensing")]
        public IActionResult Dispensing()
        {
            var records = _pharmacyService.GetPharmacyRecords();
            ViewBag.Patients = _patientService.GetPatients();
            ViewBag.MedicineStock = _medicineStockService.MedicineStock;
            return View(records);
        }

        [HttpPost]
        [Route("pharmacist/dispense")]
        public IActionResult Dispense(int recordId)
        {
            var pharmacistName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Rahul Verma";
            var records = _pharmacyService.GetPharmacyRecords();
            var record = records.FirstOrDefault(r => r.PharmacyRecordId == recordId);

            if (record == null)
            {
                TempData["ErrorMessage"] = "Prescription record not found.";
                return RedirectToAction("Dispensing");
            }

            if (record.Status == "DISPENSED")
            {
                TempData["ErrorMessage"] = "Medicine already dispensed.";
                return RedirectToAction("Dispensing");
            }

            // Check stock level
            if (_medicineStockService.MedicineStock.TryGetValue(record.MedicineName, out int stock) && stock < record.Quantity)
            {
                TempData["ErrorMessage"] = $"Insufficient stock for {record.MedicineName}. Available: {stock}. Required: {record.Quantity}.";
                return RedirectToAction("Dispensing");
            }

            _pharmacyService.DispenseMedicine(recordId, pharmacistName);
            TempData["SuccessMessage"] = $"Successfully dispensed {record.MedicineName} ({record.Quantity} units) to patient.";
            return RedirectToAction("Dispensing");
        }

        [HttpPost]
        [Route("pharmacist/update-stock")]
        public IActionResult UpdateStock(string medicineName, int amount)
        {
            if (string.IsNullOrEmpty(medicineName))
            {
                TempData["ErrorMessage"] = "Medicine name is required.";
                return RedirectToAction("Dispensing");
            }

            _medicineStockService.UpdateMedicineStock(medicineName, amount);
            string actionText = amount >= 0 ? "added to" : "deducted from";
            TempData["SuccessMessage"] = $"Successfully {actionText} stock for '{medicineName}' by {Math.Abs(amount)} units.";
            return RedirectToAction("Dispensing");
        }
    }
}
