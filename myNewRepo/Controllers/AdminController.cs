using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Route("admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : BaseController
    {
        public AdminController()
        {
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();

            var model = new DashboardViewModel
            {
                UserDisplayName = User.Identity?.Name ?? "System Administrator",
                TotalPatients = 1245,
                AdmittedPatients = 42,
                TotalDoctors = users.Count(u => u.Role == "doctor"),
                TotalRevenue = 2850000m,
                CompletedLabOrders = 156,
                PendingLabOrders = 23,
                ActiveTreatments = 89,
                RecentAdmissions = new List<Patient>
                {
                    new Patient { PatientId = 1001, Name = "Rahul Sharma", Age = 45, Gender = "Male", Status = "ADMITTED", CreatedDate = DateTime.Now.AddDays(-1) },
                    new Patient { PatientId = 1002, Name = "Priya Patel", Age = 32, Gender = "Female", Status = "DISCHARGED", CreatedDate = DateTime.Now.AddDays(-2) },
                    new Patient { PatientId = 1003, Name = "Amit Kumar", Age = 28, Gender = "Male", Status = "OBSERVATION", CreatedDate = DateTime.Now }
                },
                RecentActivities = new List<string>
                {
                    "System booted and checked successfully.",
                    "Administrator logged into the dashboard.",
                    "Daily background backups completed."
                }
            };

            return View(model);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> Doctors()
        {
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();

            ViewBag.DoctorsList = users.Where(u => u.Role == "doctor").Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                FullName = u.FullName,
                Specialty = u.Specialty,
                Biography = u.Biography,
                ContactNumber = u.ContactNumber,
                Email = u.Email
            }).ToList();

            return View();
        }

        [HttpPost("hire-doctor")]
        public async Task<IActionResult> HireDoctor(string fullName, string username, string password, string specialty, string contactNumber, string email, string biography)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Full Name and Username are required.";
                return RedirectToAction(nameof(Doctors));
            }

            var payload = new UserCreateDto
            {
                FullName = fullName,
                Username = username,
                Password = string.IsNullOrWhiteSpace(password) ? "password" : password,
                Role = "doctor",
                Specialty = string.IsNullOrWhiteSpace(specialty) ? null : specialty,
                Biography = string.IsNullOrWhiteSpace(biography) ? null : biography,
                ContactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber,
                Email = string.IsNullOrWhiteSpace(email) ? null : email
            };

            var response = await SendAsync("api/admin/users", HttpMethod.Post, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["Success"] = $"Dr. {fullName} has been successfully hired.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["Error"] = !string.IsNullOrEmpty(error) ? error : "Failed to hire doctor via API.";
            }

            return RedirectToAction(nameof(Doctors));
        }

        [HttpPost("remove-doctor")]
        public async Task<IActionResult> RemoveDoctor(int doctorId)
        {
            var response = await SendAsync($"api/admin/users/{doctorId}", HttpMethod.Delete);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Doctor successfully removed from the registry.";
            }
            else
            {
                TempData["Error"] = "Failed to remove doctor via API.";
            }

            return RedirectToAction(nameof(Doctors));
        }

        [HttpGet("staff")]
        public async Task<IActionResult> Staff()
        {
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();

            ViewBag.StaffList = users.Where(u => u.Role == "receptionist" ||
                                                u.Role == "laboratory" ||
                                                u.Role == "pharmacist" ||
                                                u.Role == "pharmiacist" ||
                                                u.Role == "billing discharge").Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                FullName = u.FullName,
                Specialty = u.Specialty,
                Biography = u.Biography,
                ContactNumber = u.ContactNumber,
                Email = u.Email
            }).ToList();

            return View();
        }

        [HttpPost("hire-staff")]
        public async Task<IActionResult> HireStaff(string fullName, string username, string role, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Error"] = "All fields are required to hire staff.";
                return RedirectToAction(nameof(Staff));
            }

            var payload = new UserCreateDto
            {
                FullName = fullName,
                Username = username,
                Password = string.IsNullOrWhiteSpace(password) ? "password" : password,
                Role = role,
                Specialty = null,
                Biography = null,
                ContactNumber = null,
                Email = null
            };

            var response = await SendAsync("api/admin/users", HttpMethod.Post, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["Success"] = $"{fullName} has been hired as {role}.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["Error"] = !string.IsNullOrEmpty(error) ? error : "Failed to hire staff via API.";
            }

            return RedirectToAction(nameof(Staff));
        }

        [HttpPost("remove-staff")]
        public async Task<IActionResult> RemoveStaff(int userId)
        {
            var response = await SendAsync($"api/admin/users/{userId}", HttpMethod.Delete);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Staff member successfully removed.";
            }
            else
            {
                TempData["Error"] = "Failed to remove staff via API.";
            }

            return RedirectToAction(nameof(Staff));
        }
    }
}
