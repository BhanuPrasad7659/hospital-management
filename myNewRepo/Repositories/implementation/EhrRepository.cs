using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class EhrRepository : Repository<EhrRecord>, IEhrRepository
    {
        private readonly HospitalDbContext _context;

        public EhrRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public List<EhrRecord> GetEhrForPatient(int patientId)
        {
            return Find(e => e.PatientId == patientId).ToList();
        }

        public EhrRecord CreateEhr(EhrRecord record)
        {
            if (record.DoctorId.HasValue && record.DoctorId.Value > 0 && string.IsNullOrWhiteSpace(record.DoctorName))
            {
                var doctor = _context.Users.FirstOrDefault(u => u.Id == record.DoctorId.Value);
                if (doctor != null)
                {
                    record.DoctorName = doctor.FullName;
                }
            }
            if (record.PatientId > 0 && string.IsNullOrWhiteSpace(record.PatientName))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.PatientId == record.PatientId);
                if (patient != null)
                {
                    record.PatientName = patient.Name;
                }
            }
            record.VisitDate = DateTime.Now;
            Add(record);
            SaveChanges();
            return record;
        }
    }
}
