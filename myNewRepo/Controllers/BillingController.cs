using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,billing discharge")]
    public class BillingController : BaseController
    {
        public BillingController()
        {
        }

        [Route("billing/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var bills = await GetAsync<List<BillingRecord>>("api/billing") ?? new List<BillingRecord>();

            var pending = bills.Count(b => b.Status == "PENDING");
            var paid = bills.Count(b => b.Status == "PAID");
            var totalRevenue = bills.Where(b => b.Status == "PAID").Sum(b => b.TotalAmount);

            var viewModel = new DashboardViewModel
            {
                TotalPatients = bills.Count,
                ActiveCases = pending,
                ActiveTreatments = paid,
                TotalRevenue = totalRevenue,
                RecentActivities = new List<string>
                {
                    "Bill generated for Rakesh Patel (Pending).",
                    "Collected ₹3,050 from Amit Sharma.",
                    "Patient Amit Sharma discharged by Billing Officer."
                },
                RoleName = "Billing Operations Desk",
                UserDisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Billing Officer"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("billing/payments")]
        public async Task<IActionResult> Payments(int? patientId)
        {
            var patientDtos = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();

            var patients = patientDtos.Select(p => new Patient
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

            ViewBag.PatientsList = patients;

            BillingViewModel? viewModel = null;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = patients.FirstOrDefault(p => p.PatientId == patientId.Value);

                var bill = await GetAsync<BillingRecord>($"api/billing/patient/{patientId.Value}");
                if (bill != null)
                {
                    viewModel = new BillingViewModel
                    {
                        BillingRecordId = bill.BillingRecordId,
                        PatientId = bill.PatientId,
                        PatientName = selectedPatient?.Name ?? "Unknown",
                        ConsultationFee = bill.ConsultationFee,
                        LabCharges = bill.LabCharges,
                        MedicineCharges = bill.MedicineCharges,
                        RoomCharges = bill.RoomCharges,
                        Status = bill.Status,
                        IsDischarged = bill.IsDischarged,
                        DischargeRemarks = bill.DischargeRemarks,
                        Patient = selectedPatient
                    };
                }
                else
                {
                    decimal room = selectedPatient?.Status == "ADMITTED" ? 1500 : 0;
                    decimal lab = 0;
                    decimal medicine = 0;

                    var labResponse = await GetAsync<List<OrderTestLab>>($"api/lab/orders/patient/{patientId.Value}");
                    if (labResponse != null)
                    {
                        lab = labResponse.Count * 800;
                    }

                    var pharmResponse = await GetAsync<List<PharmacyRecord>>("api/pharmacist/records");
                    if (pharmResponse != null)
                    {
                        var patientPrescriptions = pharmResponse.Where(p => p.PatientId == patientId.Value).ToList();
                        var pricesResponse = await GetAsync<Dictionary<string, decimal>>("api/pharmacist/prices");
                        if (pricesResponse != null)
                        {
                            medicine = patientPrescriptions.Sum(p => {
                                var cleanName = p.MedicineName?.Trim() ?? "";
                                if (pricesResponse.TryGetValue(cleanName, out decimal pPrice))
                                {
                                    return pPrice * p.Quantity;
                                }
                                return 10.00m * p.Quantity;
                            });
                        }
                    }

                    viewModel = new BillingViewModel
                    {
                        PatientId = patientId.Value,
                        PatientName = selectedPatient?.Name ?? "Unknown",
                        ConsultationFee = 500,
                        LabCharges = lab,
                        MedicineCharges = medicine,
                        RoomCharges = room,
                        Status = "NONE",
                        Patient = selectedPatient
                    };
                }
            }

            ViewBag.SelectedPatient = selectedPatient;
            return View(viewModel);
        }

        [HttpPost]
        [Route("billing/generate")]
        public async Task<IActionResult> GenerateBill(int patientId, decimal consultationFee, decimal labCharges, decimal medicineCharges, decimal roomCharges)
        {
            var patResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId}");
            if (patResponse == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var payload = new
            {
                PatientId = patientId,
                ConsultationFee = consultationFee,
                LabCharges = labCharges,
                MedicineCharges = medicineCharges,
                RoomCharges = roomCharges
            };

            var response = await SendAsync("api/billing", HttpMethod.Post, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                var bill = await response.Content.ReadFromJsonAsync<BillingRecord>();
                TempData["SuccessMessage"] = $"Bill generated successfully for {patResponse.Name}! Total: ₹{bill?.TotalAmount ?? 0}";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to generate bill via API.";
            }

            return RedirectToAction("Payments", new { patientId = patientId });
        }

        [HttpGet]
        [Route("billing/pay-bill")]
        public async Task<IActionResult> PayBill(int billingRecordId)
        {
            if (billingRecordId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid billing record requested.";
                return RedirectToAction("Payments");
            }

            var bill = await GetAsync<BillingRecord>($"api/billing/{billingRecordId}");
            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            Patient? patient = null;
            var patResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{bill.PatientId}");
            if (patResponse != null)
            {
                patient = new Patient
                {
                    PatientId = patResponse.PatientId,
                    Name = patResponse.Name,
                    Age = patResponse.Age,
                    Gender = patResponse.Gender,
                    Status = patResponse.Status,
                    AssignedDoctorId = patResponse.AssignedDoctorId,
                    AssignedDoctorName = patResponse.AssignedDoctorName,
                    Ward = patResponse.Ward,
                    BedNumber = patResponse.BedNumber
                };
            }

            var viewModel = new BillingViewModel
            {
                BillingRecordId = bill.BillingRecordId,
                PatientId = bill.PatientId,
                PatientName = patient?.Name ?? "Unknown",
                ConsultationFee = bill.ConsultationFee,
                LabCharges = bill.LabCharges,
                MedicineCharges = bill.MedicineCharges,
                RoomCharges = bill.RoomCharges,
                Status = bill.Status,
                IsDischarged = bill.IsDischarged,
                DischargeRemarks = bill.DischargeRemarks,
                Patient = patient
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("billing/pay-bill")]
        public async Task<IActionResult> ProcessBillPayment(int billingRecordId, string paymentMode, string? upiApp, string? upiId, string? cardNumber, decimal? cashReceived)
        {
            var bill = await GetAsync<BillingRecord>($"api/billing/{billingRecordId}");
            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            string finalMethod = "Pay the Bill - Direct Settle";
            if (paymentMode == "UPI")
            {
                var app = string.IsNullOrWhiteSpace(upiApp) ? "BHIM UPI" : upiApp;
                var id = string.IsNullOrWhiteSpace(upiId) ? "" : $" ({upiId})";
                finalMethod = $"UPI - {app}{id}";
            }
            else if (paymentMode == "CASH")
            {
                var tender = cashReceived.HasValue ? $" (Received ₹{cashReceived.Value:N0})" : "";
                finalMethod = $"Cash Payment{tender}";
            }
            else if (paymentMode == "CARD")
            {
                var cardLast4 = !string.IsNullOrWhiteSpace(cardNumber) && cardNumber.Length >= 4
                    ? $" ending in {cardNumber.Substring(cardNumber.Length - 4)}"
                    : "";
                finalMethod = $"Credit/Debit Card{cardLast4}";
            }

            var payResponse = await SendAsync($"api/billing/pay/{billingRecordId}", HttpMethod.Put);
            if (payResponse != null && payResponse.IsSuccessStatusCode)
            {
                var random = new Random();
                string txnId = $"TXN{DateTime.Now:yyyyMMdd}{random.Next(100000, 999999)}";
                TempData["SuccessMessage"] = $"Payment of ₹{bill.TotalAmount:N2} processed successfully via {finalMethod}! Transaction Ref: {txnId}";
                return RedirectToAction("PrintInvoice", new { billingRecordId = billingRecordId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to process payment via API.";
            }

            return RedirectToAction("Payments");
        }

        [HttpPost]
        [Route("billing/pay")]
        public async Task<IActionResult> CollectPayment(int billingRecordId)
        {
            var bill = await GetAsync<BillingRecord>($"api/billing/{billingRecordId}");
            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            var payResponse = await SendAsync($"api/billing/pay/{billingRecordId}", HttpMethod.Put);
            if (payResponse != null && payResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Payment of ₹{bill.TotalAmount} collected successfully! Bill status: PAID.";
                return RedirectToAction("PrintInvoice", new { billingRecordId = billingRecordId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to collect payment via API.";
            }

            return RedirectToAction("Payments");
        }

        [HttpGet]
        [Route("billing/print-invoice")]
        public async Task<IActionResult> PrintInvoice(int billingRecordId)
        {
            if (billingRecordId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid invoice ID.";
                return RedirectToAction("Payments");
            }

            var bill = await GetAsync<BillingRecord>($"api/billing/{billingRecordId}");
            if (bill == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Payments");
            }

            var patResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{bill.PatientId}");
            if (patResponse != null)
            {
                bill.Patient = new Patient
                {
                    PatientId = patResponse.PatientId,
                    Name = patResponse.Name,
                    Age = patResponse.Age,
                    Gender = patResponse.Gender,
                    Status = patResponse.Status,
                    AssignedDoctorId = patResponse.AssignedDoctorId,
                    AssignedDoctorName = patResponse.AssignedDoctorName,
                    Ward = patResponse.Ward,
                    BedNumber = patResponse.BedNumber
                };
            }

            return View(bill);
        }

        [HttpPost]
        [Route("billing/discharge")]
        public async Task<IActionResult> DischargePatient(int patientId, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                remarks = "Discharged from hospital. Bill paid in full.";
            }

            var patResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId}");
            if (patResponse == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var bill = await GetAsync<BillingRecord>($"api/billing/patient/{patientId}");
            if (bill == null || bill.Status != "PAID")
            {
                TempData["ErrorMessage"] = "Cannot discharge patient. Bill is either not generated or outstanding.";
                return RedirectToAction("Payments", new { patientId = patientId });
            }

            var dischargeUrl = $"api/billing/discharge/{patientId}?remarks={Uri.EscapeDataString(remarks)}";
            var dischargeResponse = await SendAsync(dischargeUrl, HttpMethod.Put);
            if (dischargeResponse != null && dischargeResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Patient {patResponse.Name} has been discharged successfully! Status set to DISCHARGED.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to discharge patient via API.";
            }

            return RedirectToAction("Payments", new { patientId = patientId });
        }
    }
}
