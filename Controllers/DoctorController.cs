using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services.Interfaces;
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
                private readonly IOrderTestLabService _orderTestLabService;
        private readonly ITreatmentPlanService _treatmentPlanService;
        private readonly IUserService _userService;
        private readonly IEhrService _ehrService;
        private readonly IPatientService _patientService;
        private readonly IMedicineStockService _medicineStockService;

        public DoctorController(IOrderTestLabService orderTestLabService, ITreatmentPlanService treatmentPlanService, IUserService userService, IEhrService ehrService, IPatientService patientService, IMedicineStockService medicineStockService)
        {
            _orderTestLabService = orderTestLabService;
            _treatmentPlanService = treatmentPlanService;
            _userService = userService;
            _ehrService = ehrService;
            _patientService = patientService;
            _medicineStockService = medicineStockService;
        }

        // Updated to extract the 3rd variable: doctorUniqueId
        private (int doctorId, string doctorName, string doctorUniqueId) GetCurrentDoctorInfo()
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var doctorUser = _userService.GetUsers().FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));

            int docId = doctorUser?.Id ?? (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0);
            string docName = doctorUser?.FullName ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Dr. Ramesh";

            // Grabs the newly created DoctorUniqueId, defaults to formatting the integer if missing
            string uniqueId = doctorUser?.DoctorUniqueId ?? $"D{docId:D3}";

            return (docId, docName, uniqueId);
        }

        [Route("doctor/dashboard")]
        public IActionResult Dashboard()
        {
            var (doctorId, doctorName, doctorUniqueId) = GetCurrentDoctorInfo();

            var patients = _patientService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            var treatments = _treatmentPlanService.GetTreatments()
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
                // Display the unique ID alongside the name
                UserDisplayName = $"{doctorUniqueId} - {doctorName}"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("doctor/ehr")]
        public IActionResult Ehr(int? patientId)
        {
            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            var patients = _patientService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            ViewBag.PatientsList = patients;

            List<EhrRecord> records;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _patientService.GetPatient(patientId.Value);
                records = _ehrService.GetEhrForPatient(patientId.Value);
            }
            else
            {
                records = _ehrService.GetEhrRecords();
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

            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            record.DoctorId = doctorId;
            record.DoctorName = doctorName;
            _ehrService.CreateEhr(record);

            TempData["SuccessMessage"] = "EHR saved! Please order the necessary lab tests next.";

            return RedirectToAction("OrderLabTest", new { patientId = record.PatientId });
        }

        [HttpGet]
        [Route("doctor/order-lab-test")]
        public IActionResult OrderLabTest(int? patientId)
        {
            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();

            var patientsWithEhrIds = _ehrService.GetEhrRecords()
                .Select(e => e.PatientId)
                .Distinct()
                .ToList();

            var patients = _patientService.GetPatients()
                .Where(p => p.Status == "ADMITTED" &&
                            (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))) &&
                            patientsWithEhrIds.Contains(p.PatientId))
                .ToList();

            ViewBag.PatientsList = patients;

            Patient? selectedPatient = null;
            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _patientService.GetPatient(patientId.Value);
            }

            ViewBag.SelectedPatient = selectedPatient;
            return View(new OrderTestLab { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/order-lab-test")]
        public IActionResult OrderLabTest(int patientId, List<string> selectedTests)
        {
            if (selectedTests == null || !selectedTests.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one laboratory test.";
                return RedirectToAction("OrderLabTest", new { patientId = patientId });
            }

            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            var patient = _patientService.GetPatient(patientId);
            var patientName = patient?.Name ?? "Unknown";

            foreach (var testName in selectedTests)
            {
                var order = new OrderTestLab
                {
                    PatientId = patientId,
                    PatientName = patientName,
                    DoctorId = doctorId,
                    DoctorName = doctorName,
                    TestName = testName,
                    Status = "ORDERED",
                    OrderDate = DateTime.Now
                };
                _orderTestLabService.CreateOrderTestLab(order);
            }

            TempData["SuccessMessage"] = $"Ordered {selectedTests.Count} lab tests successfully! You can start treatment once results are uploaded.";

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("doctor/treatment")]
        public IActionResult Treatment(int? patientId)
        {
            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            var patients = _patientService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            ViewBag.PatientsList = patients;

            List<TreatmentPlan> treatments;
            List<OrderTestLab> labResults;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _patientService.GetPatient(patientId.Value);
                treatments = _treatmentPlanService.GetTreatmentsForPatient(patientId.Value);
                labResults = _orderTestLabService.GetOrderTestLabsForPatient(patientId.Value);
            }
            else
            {
                treatments = _treatmentPlanService.GetTreatments();
                labResults = _orderTestLabService.GetOrderTestLabs();
            }

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.TreatmentHistory = treatments;
            ViewBag.PatientLabResults = labResults;

            // Specialty-specific medicines mapping
            var doctorUsername = User.Identity?.Name ?? "";
            var doctorUser = _userService.GetUsers().FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase));
            var specialty = doctorUser?.Specialty ?? "Physician";

            var specialtyMeds = new List<string>();
            if (specialty.Equals("Cardiology", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Atorvastatin 20mg", "Metoprolol 50mg", "Clopidogrel 75mg", "Aspirin 81mg", "Amlodipine 5mg" };
            }
            else if (specialty.Equals("Pediatrics", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Amoxicillin 250mg Suspension", "Paracetamol 120mg Syrup", "Ibuprofen 100mg Suspension", "Cetirizine 5mg Syrup" };
            }
            else if (specialty.Equals("Orthopedics", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Diclofenac 50mg", "Ibuprofen 400mg", "Tramadol 50mg", "Calcium + Vitamin D3" };
            }
            else if (specialty.Equals("Neurology", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Gabapentin 300mg", "Levetiracetam 500mg", "Donepezil 5mg", "Sumatriptan 50mg" };
            }
            else
            {
                specialtyMeds = new List<string> { "Amoxicillin 500mg", "Paracetamol 650mg", "Azithromycin 500mg", "Pantoprazole 40mg", "Cetirizine 10mg" };
            }

            var dbStock = _medicineStockService.MedicineStock;
            var availableMeds = specialtyMeds.Where(m => dbStock.ContainsKey(m)).ToList();
            ViewBag.AvailableMedicines = availableMeds;

            return View(new TreatmentPlan { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/treatment")]
        public IActionResult AddTreatment(TreatmentPlan plan, List<string> selectedMedicines)
        {
            if (plan.PatientId <= 0 || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                TempData["ErrorMessage"] = "Patient selection and Treatment Description are required.";
                return RedirectToAction("Treatment", new { patientId = plan.PatientId });
            }

            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            plan.DoctorId = doctorId;
            plan.DoctorName = doctorName;
            plan.Medication = selectedMedicines != null && selectedMedicines.Any()
                ? string.Join(", ", selectedMedicines)
                : "";

            try
            {
                _treatmentPlanService.CreateTreatmentPlan(plan);
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
            var (doctorId, doctorName, _) = GetCurrentDoctorInfo();
            var patients = _patientService.GetPatients()
                .Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            ViewBag.PatientsList = patients;

            Patient? selectedPatient = null;
            List<TreatmentPlan> treatments;
            List<OrderTestLab> labOrders;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _patientService.GetPatient(patientId.Value);
                treatments = _treatmentPlanService.GetTreatmentsForPatient(patientId.Value);
                labOrders = _orderTestLabService.GetOrderTestLabsForPatient(patientId.Value);
            }
            else
            {
                treatments = _treatmentPlanService.GetTreatments();
                labOrders = _orderTestLabService.GetOrderTestLabs();
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
            var (doctorId, _, _) = GetCurrentDoctorInfo();
            var doctor = _userService.GetUserById(doctorId);
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
            var (doctorId, _, _) = GetCurrentDoctorInfo();
            _userService.UpdateDoctorProfile(doctorId, specialty, biography, contactNumber, email);
            TempData["SuccessMessage"] = "Professional doctor profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("doctor/view-profile")]
        public IActionResult ViewProfile(int doctorId)
        {
            var doctor = _userService.GetDoctorById(doctorId);
            if (doctor == null)
            {
                return Content("Doctor profile not found.");
            }
            return View(doctor);
        }
    }
}