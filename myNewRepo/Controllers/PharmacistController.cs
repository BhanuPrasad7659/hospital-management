using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,pharmacist,pharmiacist")]
    public class PharmacistController : BaseController
    {
        public PharmacistController()
        {
        }

        [Route("pharmacist/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var records = await GetAsync<List<PharmacyRecord>>("api/pharmacist/records") ?? new List<PharmacyRecord>();
            var stock = await GetAsync<Dictionary<string, int>>("api/pharmacist/stock") ?? new Dictionary<string, int>();

            var pending = records.Count(r => r.Status == "PENDING");
            var dispensed = records.Count(r => r.Status == "DISPENSED");
            var totalStock = stock.Values.Sum();

            var viewModel = new DashboardViewModel
            {
                TotalPatients = totalStock,
                ActiveCases = dispensed,
                ActiveTreatments = pending,
                RecentActivities = new List<string>
                {
                    "Dispensed Amoxicillin 500mg (14 units) to Patient Amit Sharma.",
                    "Restocked Metformin 500mg (+100 units).",
                    "Received new prescription order from Dr. Ramesh."
                },
                RoleName = "Pharmacy Dispensary",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Pharmacist"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("pharmacist/dispensing")]
        public async Task<IActionResult> Dispensing()
        {
            var records = await GetAsync<List<PharmacyRecord>>("api/pharmacist/records") ?? new List<PharmacyRecord>();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            var stock = await GetAsync<Dictionary<string, int>>("api/pharmacist/stock") ?? new Dictionary<string, int>();

            ViewBag.Patients = patients.Select(p => new Patient
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
            ViewBag.MedicineStock = stock;
            return View(records);
        }

        [HttpPost]
        [Route("pharmacist/dispense")]
        public async Task<IActionResult> Dispense(int recordId)
        {
            var pharmacistName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Rahul Verma";
            var path = $"api/pharmacist/dispense/{recordId}?pharmacistName={Uri.EscapeDataString(pharmacistName)}";
            var response = await SendAsync(path, HttpMethod.Put);

            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully dispensed medicine to patient.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to dispense medicine via API.";
            }
            return RedirectToAction("Dispensing");
        }

        [HttpPost]
        [Route("pharmacist/update-stock")]
        public async Task<IActionResult> UpdateStock(string medicineName, int amount)
        {
            if (string.IsNullOrEmpty(medicineName))
            {
                TempData["ErrorMessage"] = "Medicine name is required.";
                return RedirectToAction("Dispensing");
            }

            var payload = new { MedicineName = medicineName, Amount = amount };
            var response = await SendAsync("api/pharmacist/stock", HttpMethod.Post, payload);

            if (response != null && response.IsSuccessStatusCode)
            {
                string actionText = amount >= 0 ? "added to" : "deducted from";
                TempData["SuccessMessage"] = $"Successfully {actionText} stock for '{medicineName}' by {Math.Abs(amount)} units.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to update stock via API.";
            }
            return RedirectToAction("Dispensing");
        }
    }
}
