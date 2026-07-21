using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly HospitalService _hospitalService;

        public ReportsController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [Route("reports")]
        public IActionResult Index()
        {
            var patients = _hospitalService.GetPatients();
            var admissions = _hospitalService.GetAdmissions();
            var bills = _hospitalService.GetBillingRecords();
            var labOrders = _hospitalService.GetLabOrders();
            var pharmacy = _hospitalService.GetPharmacyRecords();

            // 1. Monthly Revenue Analytics
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Reverse()
                .ToList();

            var monthlyLabels = last6Months.Select(m => m.ToString("MMM")).ToList();
            var monthlyValues = _hospitalService.GetMonthlyRevenue(last6Months);

            // 2. Admissions by Department/Ward Data
            var wardData = _hospitalService.GetWardAdmissionsData();

            // 3. Lab Test Volumes Data
            var labData = _hospitalService.GetLabTestVolumesData();

            // 4. Recent Paid Invoices Feed
            ViewBag.RecentPaidBills = _hospitalService.GetRecentPaidBills(5);

            // 5. Departmental Wards Summary
            ViewBag.WardSummary = _hospitalService.GetWardSummaries();

            var viewModel = new ReportViewModel
            {
                TotalRevenue = _hospitalService.GetTotalPaidRevenue(),
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
