using System.Collections.Generic;

namespace CogMediHospitalManagementSystem.ViewModels
{
    public class ReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalAdmissions { get; set; }
        public int TotalLabTests { get; set; }
        public int TotalDispensedMedicines { get; set; }

        public List<string> MonthlyRevenueLabels { get; set; } = new();
        public List<decimal> MonthlyRevenueValues { get; set; } = new();
        
        public List<string> DepartmentLabels { get; set; } = new();
        public List<int> DepartmentValues { get; set; } = new();
        
        public List<string> LabTestLabels { get; set; } = new();
        public List<int> LabTestValues { get; set; } = new();
    }
}
