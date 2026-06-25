using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "receptionist")]
    public class ReceptionistController : Controller
    {
        private readonly HospitalService _hospitalService;

        public ReceptionistController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("receptionist/dashboard")]
        public IActionResult Dashboard()
        {
            var patients = _hospitalService.GetPatients();
            
            var viewModel = new DashboardViewModel
            {
                TotalPatients = patients.Count,
                AdmittedPatients = patients.Count(p => p.Status == "ADMITTED"),
                RegisteredPatients = patients.Count(p => p.Status == "REGISTERED"),
                DischargedPatients = patients.Count(p => p.Status == "DISCHARGED"),
                RecentAdmissions = patients.Where(p => p.Status == "ADMITTED").OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "New patient registered at front desk.",
                    "Bed G-15 assigned in General Ward.",
                    "Registered patient profile updated."
                },
                RoleName = "Receptionist Desk",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Receptionist"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("receptionist/admission")]
        public IActionResult Admission()
        {
            var patients = _hospitalService.GetPatients();
            ViewBag.PatientsList = patients;
            return View(new PatientViewModel());
        }

        [HttpPost]
        [Route("receptionist/register")]
        public IActionResult Register(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var patient = new Patient
                {
                    Name = model.Name,
                    Age = model.Age,
                    Gender = model.Gender,
                    Address = model.Address,
                    ContactNumber = model.ContactNumber
                };
                _hospitalService.RegisterPatient(patient);
                TempData["SuccessMessage"] = "Patient registered successfully! ID: " + patient.PatientId;
                return RedirectToAction("Admission");
            }
            
            var patients = _hospitalService.GetPatients();
            ViewBag.PatientsList = patients;
            return View("Admission", model);
        }

        [HttpPost]
        [Route("receptionist/admit")]
        public IActionResult Admit(string patientId, string ward, string bedNumber)
        {
            if (string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(bedNumber))
            {
                TempData["ErrorMessage"] = "All fields are required to admit a patient.";
                return RedirectToAction("Admission");
            }

            var patient = _hospitalService.GetPatient(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Admission");
            }

            if (patient.Status == "ADMITTED")
            {
                TempData["ErrorMessage"] = "Patient is already admitted.";
                return RedirectToAction("Admission");
            }

            _hospitalService.AdmitPatient(patientId, ward, bedNumber);
            TempData["SuccessMessage"] = $"Patient {patient.Name} admitted to {ward}, Bed {bedNumber} successfully!";
            return RedirectToAction("Admission");
        }

        [HttpPost]
        [Route("receptionist/edit")]
        public IActionResult Edit(Patient patient)
        {
            if (ModelState.IsValid)
            {
                _hospitalService.UpdatePatient(patient);
                TempData["SuccessMessage"] = $"Patient {patient.Name} updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update patient details. Please verify input.";
            }
            return RedirectToAction("Admission");
        }

        [HttpPost]
        [Route("receptionist/delete")]
        public IActionResult Delete(string patientId)
        {
            var patient = _hospitalService.GetPatient(patientId);
            if (patient != null)
            {
                _hospitalService.DeletePatient(patientId);
                TempData["SuccessMessage"] = "Patient record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Patient record not found.";
            }
            return RedirectToAction("Admission");
        }
    }
}
