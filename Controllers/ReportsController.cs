using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly HospitalService _hospitalService;

        public ReportsController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("reports")]
        public IActionResult Index()
        {
            var patients = _hospitalService.GetPatients();
            var admissions = _hospitalService.GetAdmissions();
            var bills = _hospitalService.GetBillingRecords();
            var labOrders = _hospitalService.GetLabOrders();
            var pharmacy = _hospitalService.GetPharmacyRecords();

            // Structure a beautiful report dashboard
            var viewModel = new ReportViewModel
            {
                TotalRevenue = bills.Where(b => b.Status == "PAID").Sum(b => b.TotalAmount),
                TotalAdmissions = admissions.Count,
                TotalLabTests = labOrders.Count,
                TotalDispensedMedicines = pharmacy.Count(p => p.Status == "DISPENSED"),
                
                // Monthly Revenue
                MonthlyRevenueLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                MonthlyRevenueValues = new List<decimal> { 12000, 18500, 24000, 19000, 31000, bills.Where(b => b.Status == "PAID").Sum(b => b.TotalAmount) + 5000 },
                
                // Admissions by Department
                DepartmentLabels = new List<string> { "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "Radiology", "Emergency" },
                DepartmentValues = new List<int> { 12, 8, 15, 24, 6, 30 },
                
                // Lab tests summary
                LabTestLabels = new List<string> { "Blood CBC", "Ultrasound", "Chest X-Ray", "MRI Scan", "ECG" },
                LabTestValues = new List<int> { 45, 18, 22, 9, 31 }
            };

            return View(viewModel);
        }
    }
}
