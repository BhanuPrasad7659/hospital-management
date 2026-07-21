using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;

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
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            var treatments = _hospitalService.GetTreatments()
                .Where(t => t.DoctorId == doctorId || (t.DoctorName != null && t.DoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))
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
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
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

            TempData["SuccessMessage"] = "EHR saved! Please order the necessary lab tests next.";

            // Redirect straight to the Order Lab Test page
            return RedirectToAction("OrderLabTest", new { patientId = record.PatientId });
        }

        // --- ORDER LAB TEST ACTIONS ---
        [HttpGet]
        [Route("doctor/order-lab-test")]
        public IActionResult OrderLabTest(int? patientId)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();

            // 1. Get a list of IDs for patients who already have an EHR recorded
            var patientsWithEhrIds = _hospitalService.GetEhrRecords()
                .Select(e => e.PatientId)
                .Distinct()
                .ToList();

            // 2. Load admitted patients assigned to this doctor WHO ALSO HAVE AN EHR
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" &&
                           (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))) &&
                           patientsWithEhrIds.Contains(p.PatientId))
                .ToList();

            // 3. Pass the filtered list to the view's dropdown
            ViewBag.PatientsList = patients;

            // 4. Handle the currently selected patient
            Patient? selectedPatient = null;
            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _hospitalService.GetPatient(patientId.Value);
            }

            ViewBag.SelectedPatient = selectedPatient;
            return View(new LabOrder { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/order-lab-test")]
        public IActionResult OrderLabTest(LabOrder order)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            order.DoctorId = doctorId;
            order.DoctorName = doctorName;

            _hospitalService.CreateLabOrder(order);

            TempData["SuccessMessage"] = "Lab Test Ordered successfully! You can start treatment once results are uploaded.";

            // Redirect back to dashboard to wait for results
            return RedirectToAction("Dashboard");
        }
        // -----------------------------------

        [HttpGet]
        [Route("doctor/treatment")]
        public IActionResult Treatment(int? patientId)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            ViewBag.PatientsList = patients;

            List<TreatmentPlan> treatments;
            List<LabOrder> labResults;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _hospitalService.GetPatient(patientId.Value);
                treatments = _hospitalService.GetTreatmentsForPatient(patientId.Value);
                labResults = _hospitalService.GetLabOrdersForPatient(patientId.Value);
            }
            else
            {
                treatments = _hospitalService.GetTreatments();
                labResults = _hospitalService.GetLabOrders();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.TreatmentHistory = treatments;
            ViewBag.PatientLabResults = labResults;

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

            try
            {
                // Will throw InvalidOperationException if no COMPLETED lab order exists
                _hospitalService.CreateTreatmentPlan(plan);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Treatment", new { patientId = plan.PatientId });
            }

            TempData["SuccessMessage"] = "Treatment plan prescribed successfully!";
            return RedirectToAction("Treatment", new { patientId = plan.PatientId });
        }

        [HttpGet]
        [Route("doctor/patient-history")]
        public IActionResult PatientHistory(int? patientId)
        {
            var (doctorId, doctorName) = GetCurrentDoctorInfo();
            var patients = _hospitalService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            ViewBag.PatientsList = patients;

            Patient? selectedPatient = null;
            List<TreatmentPlan> treatments;
            List<LabOrder> labOrders;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _hospitalService.GetPatient(patientId.Value);
                treatments = _hospitalService.GetTreatmentsForPatient(patientId.Value);
                labOrders = _hospitalService.GetLabOrdersForPatient(patientId.Value);
            }
            else
            {
                treatments = _hospitalService.GetTreatments();
                labOrders = _hospitalService.GetLabOrders();
            }

            ViewBag.Patient = selectedPatient;
            ViewBag.Treatments = treatments;
            ViewBag.LabOrders = labOrders;

            return View();
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