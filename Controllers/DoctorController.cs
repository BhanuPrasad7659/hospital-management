using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,doctor")]
    public class DoctorController : Controller
    {
        private readonly HospitalService _hospitalService;

        public DoctorController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("doctor/dashboard")]
        public IActionResult Dashboard()
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && p.AssignedDoctorUsername == doctorUsername)
                .ToList();
            
            var treatments = _hospitalService.GetTreatments()
                .Where(t => t.DoctorName == (User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Dr. Harsha Vardhan"))
                .ToList();
            
            var viewModel = new DashboardViewModel
            {
                TotalPatients = patients.Count, // Today's admitted queue
                ActiveCases = patients.Count,
                ActiveTreatments = treatments.Count,
                RecentAdmissions = patients.Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "Logged into Physician & Doctor Portal.",
                    "Reviewing active patient queue assigned by reception desk."
                },
                RoleName = "Medical Practitioner Desk",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Doctor"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("doctor/ehr")]
        public IActionResult Ehr(string? patientId)
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && p.AssignedDoctorUsername == doctorUsername)
                .ToList();
            ViewBag.PatientsList = patients;

            List<EhrRecord> records;
            Patient? selectedPatient = null;

            if (!string.IsNullOrEmpty(patientId))
            {
                selectedPatient = _hospitalService.GetPatient(patientId);
                records = _hospitalService.GetEhrForPatient(patientId);
            }
            else
            {
                records = _hospitalService.GetEhrRecords();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.EhrRecords = records;
            return View(new EhrRecord { PatientId = patientId ?? "" });
        }

        [HttpPost]
        [Route("doctor/ehr")]
        public IActionResult AddEhr(EhrRecord record)
        {
            if (string.IsNullOrEmpty(record.PatientId) || string.IsNullOrEmpty(record.Diagnosis))
            {
                TempData["ErrorMessage"] = "Patient selection and Diagnosis are required.";
                return RedirectToAction("Ehr", new { patientId = record.PatientId });
            }

            var doctorUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase));
            record.DoctorName = doctorUser?.FullName ?? User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Dr. Harsha Vardhan";
            _hospitalService.CreateEhr(record);
            
            TempData["SuccessMessage"] = "Electronic Health Record created successfully!";
            return RedirectToAction("Ehr", new { patientId = record.PatientId });
        }

        [HttpGet]
        [Route("doctor/treatment")]
        public IActionResult Treatment(string? patientId)
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && p.AssignedDoctorUsername == doctorUsername)
                .ToList();
            ViewBag.PatientsList = patients;

            List<TreatmentPlan> treatments;
            Patient? selectedPatient = null;

            if (!string.IsNullOrEmpty(patientId))
            {
                selectedPatient = _hospitalService.GetPatient(patientId);
                treatments = _hospitalService.GetTreatmentsForPatient(patientId);
            }
            else
            {
                treatments = _hospitalService.GetTreatments();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.TreatmentHistory = treatments;

            return View(new TreatmentPlan { PatientId = patientId ?? "" });
        }

        [HttpPost]
        [Route("doctor/treatment")]
        public IActionResult AddTreatment(TreatmentPlan plan)
        {
            if (string.IsNullOrEmpty(plan.PatientId) || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                TempData["ErrorMessage"] = "Patient selection and Treatment Description are required.";
                return RedirectToAction("Treatment", new { patientId = plan.PatientId });
            }

            var doctorUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase));
            plan.DoctorName = doctorUser?.FullName ?? User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Dr. Harsha Vardhan";
            _hospitalService.CreateTreatmentPlan(plan);

            // If OrderTest is filled, create a lab order
            if (!string.IsNullOrWhiteSpace(plan.OrderTest))
            {
                var order = new LabOrder
                {
                    PatientId = plan.PatientId,
                    TestName = plan.OrderTest,
                    DoctorName = plan.DoctorName
                };
                _hospitalService.CreateLabOrder(order);
            }

            TempData["SuccessMessage"] = "Treatment plan prescribed successfully!";
            return RedirectToAction("Treatment", new { patientId = plan.PatientId });
        }

        [HttpGet]
        [Route("doctor/profile")]
        public IActionResult Profile()
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var doctor = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase));
            if (doctor == null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(doctor);
        }

        [HttpPost]
        [Route("doctor/profile/update")]
        public IActionResult UpdateProfile(string specialty, string biography, string contactNumber, string email)
        {
            var doctorUsername = User.Identity?.Name ?? "";
            _hospitalService.UpdateDoctorProfile(doctorUsername, specialty, biography, contactNumber, email);
            TempData["SuccessMessage"] = "Professional doctor profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("doctor/view-profile")]
        public IActionResult ViewProfile(string username)
        {
            var doctor = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            if (doctor == null)
            {
                return Content("Doctor profile not found.");
            }
            return View(doctor);
        }

        // Lab test ordering is now handled in AddTreatment. The AddLabOrder action has been removed.
    }
}
