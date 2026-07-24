using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
                private readonly IOrderTestLabService _orderTestLabService;
        private readonly IAdmissionService _admissionService;
        private readonly IPatientService _patientService;
        private readonly IBillingService _billingService;
        private readonly IPharmacyService _pharmacyService;

        public ReportsController(IOrderTestLabService orderTestLabService, IAdmissionService admissionService, IPatientService patientService, IBillingService billingService, IPharmacyService pharmacyService)
        {
            _orderTestLabService = orderTestLabService;
            _admissionService = admissionService;
            _patientService = patientService;
            _billingService = billingService;
            _pharmacyService = pharmacyService;
        }

        [Route("reports")]
        public IActionResult Index()
        {
            var patients = _patientService.GetPatients();
            var admissions = _admissionService.GetAdmissions();
            var bills = _billingService.GetBillingRecords();
            var labOrders = _orderTestLabService.GetOrderTestLabs(); // Updated
            var pharmacy = _pharmacyService.GetPharmacyRecords();

            // 1. Monthly Revenue Analytics
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Reverse()
                .ToList();

            var monthlyLabels = last6Months.Select(m => m.ToString("MMM")).ToList();
            var monthlyValues = _billingService.GetMonthlyRevenue(last6Months);

            // 2. Admissions by Department/Ward Data
            var wardData = _admissionService.GetWardAdmissionsData();

            // 3. Lab Test Volumes Data
            var labData = _orderTestLabService.GetLabTestVolumesData();

            // 4. Recent Paid Invoices Feed
            ViewBag.RecentPaidBills = _billingService.GetRecentPaidBills(5);

            // 5. Departmental Wards Summary
            ViewBag.WardSummary = _admissionService.GetWardSummaries();

            var viewModel = new ReportViewModel
            {
                TotalRevenue = _billingService.GetTotalPaidRevenue(),
                TotalAdmissions = admissions.Count,
                TotalLabTests = labOrders.Count,
                TotalDispensedMedicines = pharmacy.Count(p => p.Status.Equals("DISPENSED", StringComparison.OrdinalIgnoreCase)),

                MonthlyRevenueLabels = monthlyLabels,
                MonthlyRevenueValues = monthlyValues,

                DepartmentLabels = wardData.Labels,
                DepartmentValues = wardData.Values,

                LabTestLabels = labData.Labels,
                LabTestValues = labData.Values
            };

            return View(viewModel);
        }
    }
}