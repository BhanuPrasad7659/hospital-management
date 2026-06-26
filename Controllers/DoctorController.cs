using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "doctor")]
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
            var patients = _hospitalService.GetPatients().Where(p => p.Status == "ADMITTED").ToList();
            var treatments = _hospitalService.GetTreatments();
            
            var viewModel = new DashboardViewModel
            {
                TotalPatients = patients.Count, // Today's admitted queue
                ActiveCases = patients.Count,
                ActiveTreatments = treatments.Count,
                RecentAdmissions = patients.Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "Examined patient Amit Sharma, diagnosed Appendicitis.",
                    "Prescribed Pneumonia treatment course for Priya Patel.",
                    "Reviewed abdominal ultrasound report."
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
            var patients = _hospitalService.GetPatients().Where(p => p.Status == "ADMITTED").ToList();
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

            record.DoctorName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Dr. Harsha Vardhan";
            _hospitalService.CreateEhr(record);
            
            TempData["SuccessMessage"] = "Electronic Health Record created successfully!";
            return RedirectToAction("Ehr", new { patientId = record.PatientId });
        }

        [HttpGet]
        [Route("doctor/treatment")]
        public IActionResult Treatment(string? patientId)
        {
            var patients = _hospitalService.GetPatients().Where(p => p.Status == "ADMITTED").ToList();
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

            plan.DoctorName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Dr. Harsha Vardhan";
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

        // Lab test ordering is now handled in AddTreatment. The AddLabOrder action has been removed.
    }
}
