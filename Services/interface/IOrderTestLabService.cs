using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IOrderTestLabService
    {
        List<OrderTestLab> GetOrderTestLabs();
        List<OrderTestLab> GetOrderTestLabsForPatient(int patientId);
        OrderTestLab CreateOrderTestLab(OrderTestLab order);
        void UpdateOrderTestLabStatus(int orderId, string status, string result, string technicianName);
        void DeleteOrderTestLab(int orderId);
        LabAnalyticsData GetLabTestVolumesData();
    }
}
