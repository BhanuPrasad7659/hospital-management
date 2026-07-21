using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using System.Security.Claims;

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

        private (int doctorId, string doctorName) GetCurrentDoctorInfo()
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var doctorUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            int docId = doctorUser?.Id ?? (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0);
            string docName = doctorUser?.FullName ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Dr. Ramesh";
            return (docId, docName);
        }

        [Route("doctor/dashboard")]
        public IActionResult Dashboard()
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            var treatments = _hospitalService.GetTreatments()
                .Where(t => t.DoctorId == doctorId || t.DoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            var viewModel = new DashboardViewModel
            {
                TotalPatients = patients.Count,
                ActiveCases = patients.Count,
                ActiveTreatments = treatments.Count,
                RecentAdmissions = patients.Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "Logged into Physician & Doctor Portal.",
                    "Reviewing active patient queue assigned by reception desk."
                },
                RoleName = "Medical Practitioner Desk",
                UserDisplayName = doctorName
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("doctor/ehr")]
        public IActionResult Ehr(int? patientId)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            ViewBag.PatientsList = patients;

            List<EhrRecord> records;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _hospitalService.GetPatient(patientId.Value);
                records = _hospitalService.GetEhrForPatient(patientId.Value);
            }
            else
            {
                records = _hospitalService.GetEhrRecords();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.EhrRecords = records;
            return View(new EhrRecord { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/ehr")]
        public IActionResult AddEhr(EhrRecord record)
        {
            if (record.PatientId <= 0 || string.IsNullOrEmpty(record.Diagnosis))
            {
                TempData["ErrorMessage"] = "Patient selection and Diagnosis are required.";
                return RedirectToAction("Ehr", new { patientId = record.PatientId });
            }

            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            record.DoctorId = doctorId;
            record.DoctorName = doctorName;
            _hospitalService.CreateEhr(record);
            
            TempData["SuccessMessage"] = "Electronic Health Record created successfully!";
            return RedirectToAction("Ehr", new { patientId = record.PatientId });
        }

        [HttpGet]
        [Route("doctor/treatment")]
        public IActionResult Treatment(int? patientId)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            ViewBag.PatientsList = patients;

            List<TreatmentPlan> treatments;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _hospitalService.GetPatient(patientId.Value);
                treatments = _hospitalService.GetTreatmentsForPatient(patientId.Value);
            }
            else
            {
                treatments = _hospitalService.GetTreatments();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.TreatmentHistory = treatments;

            return View(new TreatmentPlan { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/treatment")]
        public IActionResult AddTreatment(TreatmentPlan plan)
        {
            if (plan.PatientId <= 0 || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                TempData["ErrorMessage"] = "Patient selection and Treatment Description are required.";
                return RedirectToAction("Treatment", new { patientId = plan.PatientId });
            }

            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            plan.DoctorId = doctorId;
            plan.DoctorName = doctorName;
            _hospitalService.CreateTreatmentPlan(plan);

            // If OrderTest is filled, create a lab order
            if (!string.IsNullOrWhiteSpace(plan.OrderTest))
            {
                var order = new LabOrder
                {
                    PatientId = plan.PatientId,
                    DoctorId = doctorId,
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
            var (doctorId, _) = GetCurrentDoctorInfo();
            var doctor = _hospitalService.GetUserById(doctorId);
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
            var (doctorId, _) = GetCurrentDoctorInfo();
            _hospitalService.UpdateDoctorProfile(doctorId, specialty, biography, contactNumber, email);
            TempData["SuccessMessage"] = "Professional doctor profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("doctor/view-profile")]
        public IActionResult ViewProfile(int doctorId)
        {
            var doctor = _hospitalService.GetDoctorById(doctorId);
            if (doctor == null)
            {
                return Content("Doctor profile not found.");
            }
            return View(doctor);
        }
    }
}
