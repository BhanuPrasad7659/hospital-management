using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
                private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        // ==========================================
        // 1. DASHBOARD
        // ==========================================

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                UserDisplayName = User.Identity?.Name ?? "System Administrator",
                TotalPatients = 1245,
                AdmittedPatients = 42,
                TotalDoctors = _userService.GetUsers().Count(u => u.Role == "doctor"),
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

        // ==========================================
        // 2. DOCTORS MANAGEMENT
        // ==========================================

        [HttpGet("doctors")]
        public IActionResult Doctors()
        {
            var doctors = _userService.GetUsers().Where(u => u.Role == "doctor").ToList();
            ViewBag.DoctorsList = doctors;
            return View();
        }

        // POST: /admin/hire-doctor
        [HttpPost("hire-doctor")]
        public IActionResult HireDoctor(string fullName, string username, string password, string specialty, string contactNumber, string email, string biography)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Full Name and Username are required.";
                return RedirectToAction(nameof(Doctors));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                password = "password";
            }

            // Assign baseline user profile with role "doctor"
            _userService.AssignStaff(fullName, username, "doctor", password);

            // Update specific doctor profile details
            _userService.UpdateDoctorProfile(username, specialty, biography, contactNumber, email);

            TempData["Success"] = $"Dr. {fullName} has been successfully hired.";
            return RedirectToAction(nameof(Doctors));
        }

        // POST: /admin/remove-doctor
        [HttpPost("remove-doctor")]
        public IActionResult RemoveDoctor(int doctorId)
        {
            _userService.DeleteUser(doctorId);
            TempData["Success"] = "Doctor successfully removed from the registry.";
            return RedirectToAction(nameof(Doctors));
        }

        // ==========================================
        // 3. STAFF MANAGEMENT
        // ==========================================

        [HttpGet("staff")]
        public IActionResult Staff()
        {
            var allUsers = _userService.GetUsers();

            var staffList = allUsers.Where(u => u.Role == "receptionist" ||
                                                u.Role == "laboratory" ||
                                                u.Role == "pharmacist" ||
                                                u.Role == "billing discharge").ToList();

            ViewBag.StaffList = staffList;
            return View();
        }

        [HttpPost("hire-staff")]
        public IActionResult HireStaff(string fullName, string username, string role, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Error"] = "All fields are required to hire staff.";
                return RedirectToAction(nameof(Staff));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                password = "password";
            }

            _userService.AssignStaff(fullName, username, role, password);

            TempData["Success"] = $"{fullName} has been hired as {role}.";
            return RedirectToAction(nameof(Staff));
        }

        [HttpPost("remove-staff")]
        public IActionResult RemoveStaff(int userId)
        {
            _userService.DeleteUser(userId);
            TempData["Success"] = "Staff member successfully removed.";
            return RedirectToAction(nameof(Staff));
        }
    }
}