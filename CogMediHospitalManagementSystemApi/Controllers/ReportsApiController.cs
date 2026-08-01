using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly IOrderTestLabService _orderTestLabService;
        private readonly IAdmissionService _admissionService;
        private readonly IPatientService _patientService;
        private readonly IBillingService _billingService;
        private readonly IPharmacyService _pharmacyService;

        public ReportsApiController(
            IOrderTestLabService orderTestLabService,
            IAdmissionService admissionService,
            IPatientService patientService,
            IBillingService billingService,
            IPharmacyService pharmacyService)
        {
            _orderTestLabService = orderTestLabService;
            _admissionService = admissionService;
            _patientService = patientService;
            _billingService = billingService;
            _pharmacyService = pharmacyService;
        }

        [HttpGet("analytics")]
        public IActionResult GetAnalytics()
        {
            var patients = _patientService.GetPatients();
            var admissions = _admissionService.GetAdmissions();
            var bills = _billingService.GetBillingRecords();
            var labOrders = _orderTestLabService.GetOrderTestLabs();
            var pharmacy = _pharmacyService.GetPharmacyRecords();

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Reverse()
                .ToList();

            var monthlyLabels = last6Months.Select(m => m.ToString("MMM")).ToList();
            var monthlyValues = _billingService.GetMonthlyRevenue(last6Months);
            var wardData = _admissionService.GetWardAdmissionsData();
            var labData = _orderTestLabService.GetLabTestVolumesData();
            var recentPaidBills = _billingService.GetRecentPaidBills(5);
            var wardSummaries = _admissionService.GetWardSummaries();
            var totalPaidRevenue = _billingService.GetTotalPaidRevenue();

            return Ok(new
            {
                TotalPatients = patients.Count,
                TotalAdmissions = admissions.Count,
                TotalBills = bills.Count,
                TotalLabOrders = labOrders.Count,
                TotalPharmacyRecords = pharmacy.Count,
                TotalDispensedMedicines = pharmacy.Count(p => p.Status.Equals("DISPENSED", StringComparison.OrdinalIgnoreCase)),
                TotalPaidRevenue = totalPaidRevenue,
                RevenueLabels = monthlyLabels,
                RevenueValues = monthlyValues,
                WardDistribution = wardData,
                LabTestDistribution = labData,
                RecentPaidBills = recentPaidBills,
                WardSummaries = wardSummaries
            });
        }
    }
}
