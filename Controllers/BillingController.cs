using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,billing discharge")]
    public class BillingController : Controller
    {
                private readonly IDischargeService _dischargeService;
        private readonly IBillingService _billingService;
        private readonly IPatientService _patientService;
        private readonly IPharmacyService _pharmacyService;
        private readonly IOrderTestLabService _orderTestLabService;

        public BillingController(IDischargeService dischargeService, IBillingService billingService, IPatientService patientService, IPharmacyService pharmacyService, IOrderTestLabService orderTestLabService)
        {
            _dischargeService = dischargeService;
            _billingService = billingService;
            _patientService = patientService;
            _pharmacyService = pharmacyService;
            _orderTestLabService = orderTestLabService;
        }

        [Route("billing/dashboard")]
        public IActionResult Dashboard()
        {
            var bills = _billingService.GetBillingRecords();
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
        public IActionResult Payments(int? patientId)
        {
            var patients = _patientService.GetPatients();
            ViewBag.PatientsList = patients;

            BillingViewModel? viewModel = null;
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                selectedPatient = _patientService.GetPatient(patientId.Value);
                var bill = _billingService.GetBillingForPatient(patientId.Value);

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
                    decimal room = 0;
                    decimal lab = 0;
                    decimal medicine = 0;

                    if (selectedPatient?.Status == "ADMITTED")
                    {
                        room = 1500;
                    }

                    var tests = _orderTestLabService.GetOrderTestLabsForPatient(patientId.Value); // Updated
                    lab = tests.Count * 800;

                    var prescriptions = _pharmacyService.GetPharmacyRecords().Where(p => p.PatientId == patientId.Value);
                    medicine = prescriptions.Count() * 250;

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
        public IActionResult GenerateBill(int patientId, decimal consultationFee, decimal labCharges, decimal medicineCharges, decimal roomCharges)
        {
            var patient = _patientService.GetPatient(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var bill = _billingService.GenerateBill(patientId, consultationFee, labCharges, medicineCharges, roomCharges);
            TempData["SuccessMessage"] = $"Bill generated successfully for {patient.Name}! Total: ₹{bill.TotalAmount}";
            return RedirectToAction("Payments", new { patientId = patientId });
        }

        [HttpGet]
        [Route("billing/pay-bill")]
        public IActionResult PayBill(int billingRecordId)
        {
            if (billingRecordId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid billing record requested.";
                return RedirectToAction("Payments");
            }

            var bills = _billingService.GetBillingRecords();
            var bill = bills.FirstOrDefault(b => b.BillingRecordId == billingRecordId);

            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            var patient = _patientService.GetPatient(bill.PatientId);

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
        public IActionResult ProcessBillPayment(int billingRecordId, string paymentMode, string? upiApp, string? upiId, string? cardNumber, decimal? cashReceived)
        {
            var bills = _billingService.GetBillingRecords();
            var bill = bills.FirstOrDefault(b => b.BillingRecordId == billingRecordId);

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

            var random = new Random();
            string txnId = $"TXN{DateTime.Now:yyyyMMdd}{random.Next(100000, 999999)}";

            _billingService.ProcessPayment(billingRecordId, finalMethod, txnId);

            TempData["SuccessMessage"] = $"Payment of ₹{bill.TotalAmount:N2} processed successfully via {finalMethod}! Transaction Ref: {txnId}";
            return RedirectToAction("PrintInvoice", new { billingRecordId = billingRecordId });
        }

        [HttpPost]
        [Route("billing/pay")]
        public IActionResult CollectPayment(int billingRecordId)
        {
            var bills = _billingService.GetBillingRecords();
            var bill = bills.FirstOrDefault(b => b.BillingRecordId == billingRecordId);

            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            _billingService.ProcessPayment(billingRecordId, "Pay the Bill", $"TXN{DateTime.Now:yyyyMMddHHmmss}");
            TempData["SuccessMessage"] = $"Payment of ₹{bill.TotalAmount} collected successfully! Bill status: PAID.";
            return RedirectToAction("PrintInvoice", new { billingRecordId = billingRecordId });
        }

        [HttpGet]
        [Route("billing/print-invoice")]
        public IActionResult PrintInvoice(int billingRecordId)
        {
            if (billingRecordId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid invoice ID.";
                return RedirectToAction("Payments");
            }

            var bills = _billingService.GetBillingRecords();
            var bill = bills.FirstOrDefault(b => b.BillingRecordId == billingRecordId);

            if (bill == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Payments");
            }

            if (bill.Patient == null)
            {
                bill.Patient = _patientService.GetPatient(bill.PatientId);
            }

            return View(bill);
        }

        [HttpPost]
        [Route("billing/discharge")]
        public IActionResult DischargePatient(int patientId, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                remarks = "Discharged from hospital. Bill paid in full.";
            }

            var patient = _patientService.GetPatient(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var bill = _billingService.GetBillingForPatient(patientId);
            if (bill == null || bill.Status != "PAID")
            {
                TempData["ErrorMessage"] = "Cannot discharge patient. Bill is either not generated or outstanding.";
                return RedirectToAction("Payments", new { patientId = patientId });
            }

            _dischargeService.DischargePatient(patientId, remarks);
            TempData["SuccessMessage"] = $"Patient {patient.Name} has been discharged successfully! Status set to DISCHARGED.";
            return RedirectToAction("Payments", new { patientId = patientId });
        }
    }
}