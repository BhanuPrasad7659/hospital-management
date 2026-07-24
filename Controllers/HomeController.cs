using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Services.Interfaces;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
                private readonly IPatientService _patientService;
        private readonly IUserService _userService;

        public HomeController(IPatientService patientService, IUserService userService)
        {
            _patientService = patientService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Roles()
        {
            return Redirect("/#portals");
        }

        [HttpGet]
        [Route("api/search")]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            var results = new List<object>();

            if (User.IsInRole("admin"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={p.PatientId}"
                    });

                var staff = _userService.GetUsers()
                    .Where(u => u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) || u.Username.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(u => new
                    {
                        title = u.FullName,
                        subtitle = $"Staff Member ({u.Role})",
                        url = u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase) ? $"/admin/doctors" : $"/admin/dashboard"
                    });

                results.AddRange(patients);
                results.AddRange(staff);
            }
            else if (User.IsInRole("receptionist"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query) || p.ContactNumber.Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={p.PatientId}"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("doctor"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = p.Status == "ADMITTED" ? $"/doctor/ehr?patientId={p.PatientId}" : $"/doctor/dashboard"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("billing discharge"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/billing/payments?patientId={p.PatientId}"
                    });

                results.AddRange(patients);
            }

            return Json(results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
