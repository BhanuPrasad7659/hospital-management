using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,laboratory")]
    public class LabController : Controller
    {
                private readonly IOrderTestLabService _orderTestLabService;
        private readonly IPatientService _patientService;

        public LabController(IOrderTestLabService orderTestLabService, IPatientService patientService)
        {
            _orderTestLabService = orderTestLabService;
            _patientService = patientService;
        }

        [Route("lab/dashboard")]
        public IActionResult Dashboard()
        {
            // Null check added for safety
            var orders = _orderTestLabService.GetOrderTestLabs() ?? new List<OrderTestLab>();

            var pending = orders.Count(o => o.Status == "ORDERED" || o.Status == "IN_PROGRESS");
            var completed = orders.Count(o => o.Status == "COMPLETED");

            var viewModel = new DashboardViewModel
            {
                PendingLabOrders = pending,
                CompletedLabOrders = completed,
                RecentActivities = new List<string>
                {
                    "Analyzed Blood CBC Sample for Rajesh Kumar.",
                    "Completed Ultrasound scan report for Amit Sharma.",
                    "X-Ray chest imaging completed for Priya Patel."
                },
                RoleName = "Diagnostic Lab Unit",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Lab Tech"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("lab/orders")]
        public IActionResult Orders()
        {
            var dbOrders = _orderTestLabService.GetOrderTestLabs() ?? new List<OrderTestLab>();

            // 2. Safe mapping logic to prevent the NullReferenceException. 
            // We use the null-coalescing operator (??) to ensure we NEVER attempt to cast a null value.
            var mappedLabOrders = dbOrders.Where(o => o != null).Select(o =>
            {
                var type = o.GetType();

                return new LabOrder
                {
                    // Safely checks for 'Id', 'OrderTestLabId', or 'LabOrderId', defaulting to 0 if none are found.
                    LabOrderId = (int)(type.GetProperty("Id")?.GetValue(o) ?? type.GetProperty("OrderTestLabId")?.GetValue(o) ?? type.GetProperty("LabOrderId")?.GetValue(o) ?? 0),
                    PatientId = (int)(type.GetProperty("PatientId")?.GetValue(o) ?? 0),

                    Patient = type.GetProperty("Patient")?.GetValue(o) as Patient,
                    DoctorId = type.GetProperty("DoctorId")?.GetValue(o) as int?,
                    Doctor = type.GetProperty("Doctor")?.GetValue(o) as User,

                    TestName = type.GetProperty("TestName")?.GetValue(o)?.ToString() ?? "Unknown Test",
                    Status = type.GetProperty("Status")?.GetValue(o)?.ToString() ?? "ORDERED",
                    Result = type.GetProperty("Result")?.GetValue(o)?.ToString() ?? string.Empty,

                    // Safely cast to nullable DateTime before providing the fallback
                    OrderDate = type.GetProperty("OrderDate")?.GetValue(o) as DateTime? ?? DateTime.Now,

                    DoctorName = type.GetProperty("DoctorName")?.GetValue(o)?.ToString() ?? string.Empty,
                    TechnicianName = type.GetProperty("TechnicianName")?.GetValue(o)?.ToString() ?? string.Empty
                };
            }).ToList();

            ViewBag.Patients = _patientService.GetPatients();

            return View(mappedLabOrders);
        }

        [HttpPost]
        [Route("lab/start")]
        public IActionResult StartTest(int orderId)
        {
            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            _orderTestLabService.UpdateOrderTestLabStatus(orderId, "IN_PROGRESS", "", techName);
            TempData["SuccessMessage"] = $"Lab Order #{orderId} is now IN PROGRESS.";
            return RedirectToAction("Orders");
        }

        [HttpPost]
        [Route("lab/complete")]
        public IActionResult CompleteTest(int orderId, string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["ErrorMessage"] = "Test result findings are required to complete the order.";
                return RedirectToAction("Orders");
            }

            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            _orderTestLabService.UpdateOrderTestLabStatus(orderId, "COMPLETED", result, techName);
            TempData["SuccessMessage"] = $"Lab Order #{orderId} has been COMPLETED with results.";
            return RedirectToAction("Orders");
        }
    }
}