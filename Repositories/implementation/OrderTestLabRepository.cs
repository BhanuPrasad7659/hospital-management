using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // <-- ADDED for eager loading
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class OrderTestLabRepository : Repository<OrderTestLab>, IOrderTestLabRepository
    {
        private readonly HospitalDbContext _context;

        public OrderTestLabRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public List<OrderTestLab> GetOrderTestLabsForPatient(int patientId)
        {
            // --> FIXED: Added .Include(l => l.Patient) to load the patient details
            return _context.OrderTestLabs
                .Include(l => l.Patient)
                .Where(l => l.PatientId == patientId)
                .ToList();
        }

        public OrderTestLab CreateOrderTestLab(OrderTestLab order)
        {
            // Auto-populate DoctorName if ID exists but name is missing
            if (order.DoctorId.HasValue && order.DoctorId.Value > 0 && string.IsNullOrWhiteSpace(order.DoctorName))
            {
                var doctor = _context.Users.FirstOrDefault(u => u.Id == order.DoctorId.Value);
                if (doctor != null)
                {
                    order.DoctorName = doctor.FullName;
                }
            }

            order.Status = "ORDERED";
            order.OrderDate = DateTime.Now;

            Add(order);
            SaveChanges();

            return order;
        }

        public void UpdateOrderTestLabStatus(int orderId, string status, string result, string technicianName)
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

        public void DeleteOrderTestLab(int orderId)
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

            // Default fallback if no data exists
            if (labels.Count == 0)
            {
                labels = new List<string> { "Blood CBC", "Chest X-Ray", "MRI Scan", "ECG" };
                values = new List<int> { 0, 0, 0, 0 };
            }

            return new LabAnalyticsData { Labels = labels, Values = values };
        }

        // Checks if ANY test was ordered (regardless of completion status)
        public bool HasCompletedOrderTestLab(int patientId)
        {
            return Find(l => l.PatientId == patientId).Any();
        }
    }
}