using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int AdmittedPatients { get; set; }
        public int RegisteredPatients { get; set; }
        public int DischargedPatients { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingLabOrders { get; set; }
        public int CompletedLabOrders { get; set; }
        public int ActiveTreatments { get; set; }
        public int ActiveCases { get; set; }
        public int TotalDoctors { get; set; }

        public List<Patient> RecentAdmissions { get; set; } = new();
        public List<string> RecentActivities { get; set; } = new();

        public string RoleName { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
    }
}
