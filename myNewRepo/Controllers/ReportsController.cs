using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
    {
        public ReportsController()
        {
        }

        [Route("reports")]
        public async Task<IActionResult> Index()
        {
            var analytics = await GetAsync<AnalyticsResponse>("api/reports/analytics") ?? new AnalyticsResponse();

            ViewBag.RecentPaidBills = analytics.RecentPaidBills;
            ViewBag.WardSummary = analytics.WardSummaries;

            var viewModel = new ReportViewModel
            {
                TotalRevenue = analytics.TotalPaidRevenue,
                TotalAdmissions = analytics.TotalAdmissions,
                TotalLabTests = analytics.TotalLabOrders,
                TotalDispensedMedicines = analytics.TotalDispensedMedicines,
                MonthlyRevenueLabels = analytics.RevenueLabels,
                MonthlyRevenueValues = analytics.RevenueValues,
                DepartmentLabels = analytics.WardDistribution?.Labels ?? new List<string>(),
                DepartmentValues = analytics.WardDistribution?.Values ?? new List<int>(),
                LabTestLabels = analytics.LabTestDistribution?.Labels ?? new List<string>(),
                LabTestValues = analytics.LabTestDistribution?.Values ?? new List<int>()
            };

            return View(viewModel);
        }

        public class AnalyticsResponse
        {
            public int TotalPatients { get; set; }
            public int TotalAdmissions { get; set; }
            public int TotalBills { get; set; }
            public int TotalLabOrders { get; set; }
            public int TotalPharmacyRecords { get; set; }
            public int TotalDispensedMedicines { get; set; }
            public decimal TotalPaidRevenue { get; set; }
            public List<string> RevenueLabels { get; set; } = new List<string>();
            public List<decimal> RevenueValues { get; set; } = new List<decimal>();
            public WardAnalyticsData? WardDistribution { get; set; }
            public LabAnalyticsData? LabTestDistribution { get; set; }
            public List<BillingRecord> RecentPaidBills { get; set; } = new List<BillingRecord>();
            public List<WardSummaryItem> WardSummaries { get; set; } = new List<WardSummaryItem>();
        }
    }
}
