using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Services;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly HospitalService _hospitalService;

        public HomeController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        public IActionResult Index()
        {
            // If already logged in, we can optionally redirect to their respective dashboard, 
            // but let's keep the landing page accessible.
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
                var patients = _hospitalService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient ({p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={Uri.EscapeDataString(p.PatientId)}"
                    });

                var staff = _hospitalService.GetUsers()
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
                var patients = _hospitalService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.Contains(query, StringComparison.OrdinalIgnoreCase) || p.ContactNumber.Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient ({p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={Uri.EscapeDataString(p.PatientId)}"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("doctor"))
            {
                var patients = _hospitalService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient ({p.PatientId}) - Status: {p.Status}",
                        url = p.Status == "ADMITTED" ? $"/doctor/ehr?patientId={Uri.EscapeDataString(p.PatientId)}" : $"/doctor/dashboard"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("billing discharge"))
            {
                var patients = _hospitalService.GetPatients()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.PatientId.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient ({p.PatientId}) - Status: {p.Status}",
                        url = $"/billing/payments?patientId={Uri.EscapeDataString(p.PatientId)}"
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
