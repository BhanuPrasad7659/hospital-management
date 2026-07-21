using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class LabAnalyticsData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();
    }

    public interface ILabOrderRepository : IRepository<LabOrder>
    {
        List<LabOrder> GetLabOrdersForPatient(int patientId);
        LabOrder CreateLabOrder(LabOrder order);
        void UpdateLabOrderStatus(int orderId, string status, string result, string technicianName);
        void DeleteLabOrder(int orderId);
        LabAnalyticsData GetLabTestVolumesData();

        // Interface method to check for completed lab orders
        bool HasCompletedLabOrder(int patientId);
    }
}