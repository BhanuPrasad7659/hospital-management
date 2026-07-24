using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public class LabAnalyticsData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();
    }

    public interface IOrderTestLabRepository : IRepository<OrderTestLab>
    {
        List<OrderTestLab> GetOrderTestLabsForPatient(int patientId);
        OrderTestLab CreateOrderTestLab(OrderTestLab order);
        void UpdateOrderTestLabStatus(int orderId, string status, string result, string technicianName);
        void DeleteOrderTestLab(int orderId);
        LabAnalyticsData GetLabTestVolumesData();

        // Interface method to check for completed lab orders
        bool HasCompletedOrderTestLab(int patientId);
    }
}