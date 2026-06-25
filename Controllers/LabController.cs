using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "laboratory")]
    public class LabController : Controller
    {
        private readonly HospitalService _hospitalService;

        public LabController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("lab/dashboard")]
        public IActionResult Dashboard()
        {
            var orders = _hospitalService.GetLabOrders();
            var pending = orders.Count(o => o.Status == "ORDERED" || o.Status == "IN_PROGRESS");
            var completed = orders.Count(o => o.Status == "COMPLETED");

            var viewModel = new DashboardViewModel
            {
                PendingLabOrders = pending,
                CompletedLabOrders = completed,
                RecentActivities = new List<string>
                {
                    "Analyzed Blood CBC Sample for Rajesh Kumar.",
                    "Completed Ultrasound scan report for Amit Sharma.",
                    "X-Ray chest imaging completed for Priya Patel."
                },
                RoleName = "Diagnostic Lab Unit",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Lab Tech"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("lab/orders")]
        public IActionResult Orders()
        {
            var orders = _hospitalService.GetLabOrders();
            // We want to pass patient info along, so let's pre-populate the patients list in ViewBag
            ViewBag.Patients = _hospitalService.GetPatients();
            return View(orders);
        }

        [HttpPost]
        [Route("lab/start")]
        public IActionResult StartTest(string orderId)
        {
            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            _hospitalService.UpdateLabOrderStatus(orderId, "IN_PROGRESS", "", techName);
            TempData["SuccessMessage"] = $"Lab Order {orderId} is now IN PROGRESS.";
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [Route("lab/complete")]
        public IActionResult CompleteTest(string orderId, string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["ErrorMessage"] = "Test result findings are required to complete the order.";
                return RedirectToAction("Orders");
            }

            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            _hospitalService.UpdateLabOrderStatus(orderId, "COMPLETED", result, techName);
            TempData["SuccessMessage"] = $"Lab Order {orderId} has been COMPLETED with results.";
            return RedirectToAction("Orders");
        }
    }
}
