using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,receptionist")]
    public class ReceptionistController : BaseController
    {
        public ReceptionistController()
        {
        }

        [Route("receptionist/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();

            var patientsList = patients.Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name,
                Age = p.Age,
                Gender = p.Gender,
                Address = p.Address,
                ContactNumber = p.ContactNumber,
                Status = p.Status,
                CreatedDate = p.CreatedDate
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                TotalPatients = patientsList.Count,
                AdmittedPatients = patientsList.Count(p => p.Status == "ADMITTED"),
                RegisteredPatients = patientsList.Count(p => p.Status == "REGISTERED"),
                DischargedPatients = patientsList.Count(p => p.Status == "DISCHARGED"),
                RecentAdmissions = patientsList.Where(p => p.Status == "ADMITTED").OrderByDescending(p => p.CreatedDate).Take(5).ToList(),
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
        public async Task<IActionResult> Admission()
        {
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();
            var admissions = await GetAsync<List<Admission>>("api/receptionist/admissions") ?? new List<Admission>();

            ViewBag.PatientsList = patients.Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name,
                Age = p.Age,
                Gender = p.Gender,
                Address = p.Address,
                ContactNumber = p.ContactNumber,
                Status = p.Status,
                AssignedDoctorId = p.AssignedDoctorId,
                AssignedDoctorName = p.AssignedDoctorName,
                Ward = p.Ward,
                BedNumber = p.BedNumber
            }).ToList();

            ViewBag.DoctorsList = users.Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                FullName = u.FullName,
                Specialty = u.Specialty ?? ""
            }).ToList();

            ViewBag.OccupiedBeds = admissions.Where(a => a.Status == "ADMITTED").Select(a => a.BedNumber).ToList();
            return View(new PatientViewModel());
        }

        [HttpPost]
        [Route("receptionist/register")]
        public async Task<IActionResult> Register(PatientViewModel model)
        {
            if (model.Age <= 0)
            {
                ModelState.AddModelError("Age", "Age must be strictly greater than 0.");
            }

            if (ModelState.IsValid)
            {
                var payload = new PatientCreateDto
                {
                    Name = model.Name,
                    Age = model.Age,
                    Gender = model.Gender,
                    Address = model.Address,
                    ContactNumber = model.ContactNumber
                };

                var response = await SendAsync("api/receptionist/patients", HttpMethod.Post, payload);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<PatientDto>();
                    TempData["SuccessMessage"] = "Patient registered successfully! ID: #" + (created?.PatientId ?? 0);
                    return RedirectToAction("Admission");
                }
                else
                {
                    var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                    TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to register patient via API.";
                }
            }

            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();
            var admissions = await GetAsync<List<Admission>>("api/receptionist/admissions") ?? new List<Admission>();

            ViewBag.PatientsList = patients.Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name,
                Age = p.Age,
                Gender = p.Gender,
                Address = p.Address,
                ContactNumber = p.ContactNumber,
                Status = p.Status,
                AssignedDoctorId = p.AssignedDoctorId,
                AssignedDoctorName = p.AssignedDoctorName,
                Ward = p.Ward,
                BedNumber = p.BedNumber
            }).ToList();

            ViewBag.DoctorsList = users.Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                FullName = u.FullName,
                Specialty = u.Specialty ?? ""
            }).ToList();

            ViewBag.OccupiedBeds = admissions.Where(a => a.Status == "ADMITTED").Select(a => a.BedNumber).ToList();
            TempData["ErrorMessage"] = "Please correct the highlighted errors.";
            return View("Admission", model);
        }

        [HttpPost]
        [Route("receptionist/admit")]
        public async Task<IActionResult> Admit(int patientId, string ward, string bedNumber, int doctorId)
        {
            if (patientId <= 0 || string.IsNullOrEmpty(ward) || string.IsNullOrEmpty(bedNumber) || doctorId <= 0)
            {
                TempData["ErrorMessage"] = "All fields are required to admit a patient (including assigned doctor).";
                return RedirectToAction("Admission");
            }

            var payload = new
            {
                PatientId = patientId,
                Ward = ward,
                BedNumber = bedNumber,
                AssignedDoctorId = doctorId
            };

            var response = await SendAsync("api/receptionist/admissions", HttpMethod.Post, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Patient admitted and assigned successfully!";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to admit patient via API.";
            }

            return RedirectToAction("Admission");
        }

        [HttpPost]
        [Route("receptionist/edit")]
        public async Task<IActionResult> Edit(Patient patient)
        {
            if (patient.Age <= 0)
            {
                TempData["ErrorMessage"] = "Failed to update patient details. Age must be greater than 0.";
                return RedirectToAction("Admission");
            }

            var payload = new PatientUpdateDto
            {
                Name = patient.Name,
                Age = patient.Age,
                Gender = patient.Gender,
                Address = patient.Address,
                ContactNumber = patient.ContactNumber,
                Status = string.IsNullOrEmpty(patient.Status) ? "REGISTERED" : patient.Status
            };

            var response = await SendAsync($"api/receptionist/patients/{patient.PatientId}", HttpMethod.Put, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Patient {patient.Name} updated successfully!";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to update patient via API.";
            }

            return RedirectToAction("Admission");
        }

        [HttpPost]
        [Route("receptionist/delete")]
        public async Task<IActionResult> Delete(int patientId)
        {
            var response = await SendAsync($"api/receptionist/patients/{patientId}", HttpMethod.Delete);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Patient record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Patient record not found or failed to delete via API.";
            }

            return RedirectToAction("Admission");
        }

        [HttpGet]
        [Route("receptionist/profile")]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name ?? "";
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();

            var receptionistDto = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (receptionistDto == null)
            {
                return RedirectToAction("Dashboard");
            }

            var receptionist = new User
            {
                Id = receptionistDto.Id,
                Username = receptionistDto.Username,
                Role = receptionistDto.Role,
                FullName = receptionistDto.FullName,
                ContactNumber = receptionistDto.ContactNumber,
                Email = receptionistDto.Email
            };

            return View(receptionist);
        }

        [HttpPost]
        [Route("receptionist/profile/update")]
        public async Task<IActionResult> UpdateProfile(string fullName, string contactNumber, string email)
        {
            var username = User.Identity?.Name ?? "";

            if (string.IsNullOrEmpty(fullName))
            {
                TempData["ErrorMessage"] = "Full Name is required.";
                return RedirectToAction("Profile");
            }

            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();
            var target = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                var payload = new UserUpdateDto
                {
                    FullName = fullName,
                    ContactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email
                };
                var updateResponse = await SendAsync($"api/admin/users/{target.Id}", HttpMethod.Put, payload);
                if (updateResponse != null && updateResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update profile via API.";
                }
            }

            return RedirectToAction("Profile");
        }
    }
}
