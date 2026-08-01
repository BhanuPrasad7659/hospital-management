using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,laboratory")]
    public class LabController : BaseController
    {
        public LabController()
        {
        }

        [Route("lab/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var orders = await GetAsync<List<OrderTestLab>>("api/lab/orders") ?? new List<OrderTestLab>();

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
        public async Task<IActionResult> Orders()
        {
            var dbOrders = await GetAsync<List<OrderTestLab>>("api/lab/orders") ?? new List<OrderTestLab>();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();

            var mappedLabOrders = dbOrders.Where(o => o != null).Select(o => new LabOrder
            {
                LabOrderId = o.LabOrderId,
                PatientId = o.PatientId,
                Patient = o.Patient,
                DoctorId = o.DoctorId,
                Doctor = null,
                TestName = o.TestName,
                Status = o.Status,
                Result = o.Result,
                OrderDate = o.OrderDate,
                DoctorName = o.DoctorName,
                TechnicianName = o.TechnicianName
            }).ToList();

            ViewBag.Patients = patients.Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name
            }).ToList();

            return View(mappedLabOrders);
        }

        [HttpPost]
        [Route("lab/start")]
        public async Task<IActionResult> StartTest(int orderId)
        {
            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            var order = await GetAsync<OrderTestLab>($"api/lab/orders/{orderId}");
            if (order == null)
            {
                TempData["ErrorMessage"] = "Lab order not found.";
                return RedirectToAction("Orders");
            }

            var payload = new OrderTestLab
            {
                LabOrderId = order.LabOrderId,
                PatientId = order.PatientId,
                PatientName = order.PatientName,
                TestName = order.TestName,
                DoctorId = order.DoctorId,
                DoctorName = order.DoctorName,
                OrderDate = order.OrderDate,
                Status = "IN_PROGRESS",
                TechnicianName = techName
            };

            var response = await SendAsync($"api/lab/orders/{orderId}", HttpMethod.Put, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Lab Order #{orderId} is now IN PROGRESS.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update lab order via API.";
            }

            return RedirectToAction("Orders");
        }

        [HttpPost]
        [Route("lab/complete")]
        public async Task<IActionResult> CompleteTest(int orderId, string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                TempData["ErrorMessage"] = "Test result findings are required to complete the order.";
                return RedirectToAction("Orders");
            }

            var techName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Karan Malhotra";
            var order = await GetAsync<OrderTestLab>($"api/lab/orders/{orderId}");
            if (order == null)
            {
                TempData["ErrorMessage"] = "Lab order not found.";
                return RedirectToAction("Orders");
            }

            var payload = new OrderTestLab
            {
                LabOrderId = order.LabOrderId,
                PatientId = order.PatientId,
                PatientName = order.PatientName,
                TestName = order.TestName,
                DoctorId = order.DoctorId,
                DoctorName = order.DoctorName,
                OrderDate = order.OrderDate,
                Status = "COMPLETED",
                Result = result,
                TechnicianName = techName
            };

            var response = await SendAsync($"api/lab/orders/{orderId}", HttpMethod.Put, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Lab Order #{orderId} has been COMPLETED with results.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to complete lab order via API.";
            }

            return RedirectToAction("Orders");
        }
    }
}
