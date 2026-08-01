using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class OrderTestLabService : IOrderTestLabService
    {
        private readonly IOrderTestLabRepository _orderTestLabRepository;

        public OrderTestLabService(IOrderTestLabRepository orderTestLabRepository)
        {
            _orderTestLabRepository = orderTestLabRepository;
        }

        public List<OrderTestLab> GetOrderTestLabs() => (List<OrderTestLab>)_orderTestLabRepository.GetAll();

        public List<OrderTestLab> GetOrderTestLabsForPatient(int patientId) => _orderTestLabRepository.GetOrderTestLabsForPatient(patientId);

        public OrderTestLab CreateOrderTestLab(OrderTestLab order) => _orderTestLabRepository.CreateOrderTestLab(order);

        public void UpdateOrderTestLabStatus(int orderId, string status, string result, string technicianName)
            => _orderTestLabRepository.UpdateOrderTestLabStatus(orderId, status, result, technicianName);

        public void DeleteOrderTestLab(int orderId) => _orderTestLabRepository.DeleteOrderTestLab(orderId);

        public LabAnalyticsData GetLabTestVolumesData() => _orderTestLabRepository.GetLabTestVolumesData();
    }
}
