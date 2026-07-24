using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,receptionist")]
    public class ReceptionistController : Controller
    {
                private readonly IAdmissionService _admissionService;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;

        public ReceptionistController(IAdmissionService admissionService, IPatientService patientService, IUserService userService)
        {
            _admissionService = admissionService;
            _patientService = patientService;
            _userService = userService;
        }

        [Route("receptionist/dashboard")]
        public IActionResult Dashboard()
        {
            var patients = _patientService.GetPatients();

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
            var patients = _patientService.GetPatients();
            ViewBag.PatientsList = patients;
            ViewBag.DoctorsList = _userService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.OccupiedBeds = _admissionService.GetAdmissions().Where(a => a.Status == "ADMITTED").Select(a => a.BedNumber).ToList();
            return View(new PatientViewModel());
        }

        [HttpPost]
        [Route("receptionist/register")]
        public IActionResult Register(PatientViewModel model)
        {
            // NEW: Explicit server-side validation to block 0 or negative ages
            if (model.Age <= 0)
            {
                ModelState.AddModelError("Age", "Age must be strictly greater than 0.");
            }

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
                _patientService.RegisterPatient(patient);
                TempData["SuccessMessage"] = "Patient registered successfully! ID: #" + patient.PatientId;
                return RedirectToAction("Admission");
            }

            // If we hit this, validation failed. Reload the page data so the dropdowns/tables don't break.
            var patients = _patientService.GetPatients();
            ViewBag.PatientsList = patients;
            ViewBag.DoctorsList = _userService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.OccupiedBeds = _admissionService.GetAdmissions().Where(a => a.Status == "ADMITTED").Select(a => a.BedNumber).ToList();

            TempData["ErrorMessage"] = "Please correct the highlighted errors.";

            return View("Admission", model);
        }

        [HttpPost]
        [Route("receptionist/admit")]
        public IActionResult Admit(int patientId, string ward, string bedNumber, int doctorId)
        {
            if (patientId <= 0 || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(bedNumber) || doctorId <= 0)
            {
                TempData["ErrorMessage"] = "All fields are required to admit a patient (including assigned doctor).";
                return RedirectToAction("Admission");
            }

            var patient = _patientService.GetPatient(patientId);
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

            _admissionService.AdmitPatient(patientId, ward, bedNumber, doctorId);
            TempData["SuccessMessage"] = $"Patient {patient.Name} admitted and assigned successfully!";
            return RedirectToAction("Admission");
        }

        [HttpPost]
        [Route("receptionist/edit")]
        public IActionResult Edit(Patient patient)
        {
            if (patient.Age <= 0)
            {
                TempData["ErrorMessage"] = "Failed to update patient details. Age must be greater than 0.";
                return RedirectToAction("Admission");
            }

            if (ModelState.IsValid)
            {
                _patientService.UpdatePatient(patient);
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
        public IActionResult Delete(int patientId)
        {
            var patient = _patientService.GetPatient(patientId);
            if (patient != null)
            {
                _patientService.DeletePatient(patientId);
                TempData["SuccessMessage"] = "Patient record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Patient record not found.";
            }
            return RedirectToAction("Admission");
        }

        [HttpGet]
        [Route("receptionist/profile")]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name ?? "";
            var receptionist = _userService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (receptionist == null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(receptionist);
        }

        [HttpPost]
        [Route("receptionist/profile/update")]
        public IActionResult UpdateProfile(string fullName, string contactNumber, string email)
        {
            var username = User.Identity?.Name ?? "";

            if (string.IsNullOrEmpty(fullName))
            {
                TempData["ErrorMessage"] = "Full Name is required.";
                return RedirectToAction("Profile");
            }

            _userService.UpdateUserProfile(username, fullName, contactNumber ?? "", email ?? "");
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}