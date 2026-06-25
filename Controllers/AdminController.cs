using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly HospitalService _hospitalService;

        public AdminController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("admin/dashboard")]
        public IActionResult Dashboard()
        {
            var patients = _hospitalService.GetPatients();
            var admissions = _hospitalService.GetAdmissions();
            var billing = _hospitalService.GetBillingRecords();
            var labOrders = _hospitalService.GetLabOrders();
            var treatments = _hospitalService.GetTreatments();

            var viewModel = new DashboardViewModel
            {
                TotalPatients = patients.Count,
                AdmittedPatients = patients.Count(p => p.Status == "ADMITTED"),
                RegisteredPatients = patients.Count(p => p.Status == "REGISTERED"),
                DischargedPatients = patients.Count(p => p.Status == "DISCHARGED"),
                TotalRevenue = billing.Where(b => b.Status == "PAID").Sum(b => b.TotalAmount),
                PendingLabOrders = labOrders.Count(l => l.Status == "ORDERED" || l.Status == "IN_PROGRESS"),
                CompletedLabOrders = labOrders.Count(l => l.Status == "COMPLETED"),
                ActiveTreatments = treatments.Count,
                
                RecentAdmissions = patients.OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "System Admin updated staff credentials.",
                    "Receptionist registered new patient: Sunitha Rao.",
                    "Dr. Harsha Vardhan updated EHR for Patient Priya Patel.",
                    "Lab CBC Test ordered for Patient Rajesh Kumar.",
                    "Pharmacist dispensed Amoxicillin for Patient Amit Sharma.",
                    "Billing Officer collected ₹3,050 from Patient Amit Sharma.",
                    "Patient Amit Sharma discharged successfully."
                },
                RoleName = "System Administrator",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Admin"
            };

            // Pass the current staff directory to the view
            ViewBag.StaffList = _hospitalService.GetUsers();

            return View(viewModel);
        }

        [HttpPost]
        [Route("admin/assign-staff")]
        public IActionResult AssignStaff(string fullName, string username, string role)
        {
            if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
            {
                _hospitalService.AssignStaff(fullName, username, role.ToLower());
                TempData["SuccessMessage"] = $"Staff member '{fullName}' assigned to '{role}' successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "All fields are required to assign a staff member.";
            }
            return RedirectToAction("Dashboard");
        }
    }
}
