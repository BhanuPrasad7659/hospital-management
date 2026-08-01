using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore; 

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
            return _context.DischargeRecords
                .Include(d => d.Patient)
                .FirstOrDefault(d => d.PatientId == patientId);
        }

        public List<DischargeRecord> GetDischargeHistory()
        {
            return _context.DischargeRecords
                .Include(d => d.Patient)
                .OrderByDescending(d => d.DischargeDate)
                .ToList();
        }
    }
}
