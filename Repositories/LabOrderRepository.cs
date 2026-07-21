using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class LabOrderRepository : Repository<LabOrder>, ILabOrderRepository
    {
        public LabOrderRepository(HospitalDbContext context) : base(context)
        {
        }

        public List<LabOrder> GetLabOrdersForPatient(int patientId)
        {
            return Find(l => l.PatientId == patientId).ToList();
        }

        public LabOrder CreateLabOrder(LabOrder order)
        {
            order.Status = "ORDERED";
            order.OrderDate = DateTime.Now;
            Add(order);
            SaveChanges();
            return order;
        }

        public void UpdateLabOrderStatus(int orderId, string status, string result, string technicianName)
        {
            var order = GetById(orderId);
            if (order != null)
            {
                order.Status = status;
                if (status == "COMPLETED")
                {
                    order.Result = result;
                    order.TechnicianName = technicianName;
                }
                Update(order);
                SaveChanges();
            }
        }

        public void DeleteLabOrder(int orderId)
        {
            var order = GetById(orderId);
            if (order != null)
            {
                Delete(order);
                SaveChanges();
            }
        }

        public LabAnalyticsData GetLabTestVolumesData()
        {
            var labGroups = GetAll()
                .GroupBy(l => string.IsNullOrWhiteSpace(l.TestName) ? "Routine Test" : l.TestName)
                .Select(g => new { Test = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var labels = labGroups.Select(l => l.Test).ToList();
            var values = labGroups.Select(l => l.Count).ToList();

            if (labels.Count == 0)
            {
                labels = new List<string> { "Blood CBC", "Chest X-Ray", "MRI Scan", "ECG" };
                values = new List<int> { 0, 0, 0, 0 };
            }

            return new LabAnalyticsData { Labels = labels, Values = values };
        }
    }
}
