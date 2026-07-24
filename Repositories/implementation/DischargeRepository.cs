using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Required for Eager Loading
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class DischargeRepository : Repository<DischargeRecord>, IDischargeRepository
    {
        private readonly HospitalDbContext _context;

        public DischargeRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public DischargeRecord? GetForPatient(int patientId)
        {
            // Explicitly load the Patient data alongside the DischargeRecord
            return _context.DischargeRecords
                .Include(d => d.Patient)
                .FirstOrDefault(d => d.PatientId == patientId);
        }

        public List<DischargeRecord> GetDischargeHistory()
        {
            // Explicitly load Patient data for the entire history list
            return _context.DischargeRecords
                .Include(d => d.Patient)
                .OrderByDescending(d => d.DischargeDate)
                .ToList();
        }
    }
}