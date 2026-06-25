using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "billing discharge")]
    public class BillingController : Controller
    {
        private readonly HospitalService _hospitalService;

        public BillingController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("billing/dashboard")]
        public IActionResult Dashboard()
        {
            var bills = _hospitalService.GetBillingRecords();
            var pending = bills.Count(b => b.Status == "PENDING");
            var paid = bills.Count(b => b.Status == "PAID");
            var totalRevenue = bills.Where(b => b.Status == "PAID").Sum(b => b.TotalAmount);

            var viewModel = new DashboardViewModel
            {
                TotalPatients = bills.Count, // Repurposing for Total Bills generated
                ActiveCases = pending, // Repurposing for Pending Bills
                ActiveTreatments = paid, // Repurposing for Paid Bills
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
        public IActionResult Payments(string? patientId)
        {
            var patients = _hospitalService.GetPatients();
            ViewBag.PatientsList = patients;

            BillingViewModel? viewModel = null;
            Patient? selectedPatient = null;

            if (!string.IsNullOrEmpty(patientId))
            {
                selectedPatient = _hospitalService.GetPatient(patientId);
                var bill = _hospitalService.GetBillingForPatient(patientId);

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
                    // No bill exists, prepare default model to generate one
                    // We can precalculate some charges based on patient status to make it slick!
                    decimal room = 0;
                    decimal lab = 0;
                    decimal medicine = 0;

                    if (selectedPatient?.Status == "ADMITTED")
                    {
                        room = 1500; // Mock room charges per day
                    }

                    // Check if they have lab tests
                    var tests = _hospitalService.GetLabOrdersForPatient(patientId);
                    lab = tests.Count * 800; // ₹800 per test

                    // Check if they have prescriptions
                    var prescriptions = _hospitalService.GetPharmacyRecords().Where(p => p.PatientId == patientId);
                    medicine = prescriptions.Count() * 250; // ₹250 per prescription

                    viewModel = new BillingViewModel
                    {
                        PatientId = patientId,
                        PatientName = selectedPatient?.Name ?? "Unknown",
                        ConsultationFee = 500, // Standard doctor fee
                        LabCharges = lab,
                        MedicineCharges = medicine,
                        RoomCharges = room,
                        Status = "NONE", // Signifies no bill generated yet
                        Patient = selectedPatient
                    };
                }
            }

            ViewBag.SelectedPatient = selectedPatient;
            return View(viewModel);
        }

        [HttpPost]
        [Route("billing/generate")]
        public IActionResult GenerateBill(string patientId, decimal consultationFee, decimal labCharges, decimal medicineCharges, decimal roomCharges)
        {
            var patient = _hospitalService.GetPatient(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var bill = _hospitalService.GenerateBill(patientId, consultationFee, labCharges, medicineCharges, roomCharges);
            TempData["SuccessMessage"] = $"Bill generated successfully for {patient.Name}! Total: ₹{bill.TotalAmount}";
            return RedirectToAction("Payments", new { patientId = patientId });
        }

        [HttpPost]
        [Route("billing/pay")]
        public IActionResult CollectPayment(string billingRecordId)
        {
            var bills = _hospitalService.GetBillingRecords();
            var bill = bills.FirstOrDefault(b => b.BillingRecordId == billingRecordId);

            if (bill == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction("Payments");
            }

            _hospitalService.ProcessPayment(billingRecordId);
            TempData["SuccessMessage"] = $"Payment of ₹{bill.TotalAmount} collected successfully! Bill status: PAID.";
            return RedirectToAction("Payments", new { patientId = bill.PatientId });
        }

        [HttpPost]
        [Route("billing/discharge")]
        public IActionResult DischargePatient(string patientId, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                remarks = "Discharged from hospital. Bill paid in full.";
            }

            var patient = _hospitalService.GetPatient(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Payments");
            }

            var bill = _hospitalService.GetBillingForPatient(patientId);
            if (bill == null || bill.Status != "PAID")
            {
                TempData["ErrorMessage"] = "Cannot discharge patient. Bill is either not generated or outstanding.";
                return RedirectToAction("Payments", new { patientId = patientId });
            }

            _hospitalService.DischargePatient(patientId, remarks);
            TempData["SuccessMessage"] = $"Patient {patient.Name} has been discharged successfully! Status set to DISCHARGED.";
            return RedirectToAction("Payments", new { patientId = patientId });
        }
    }
}
