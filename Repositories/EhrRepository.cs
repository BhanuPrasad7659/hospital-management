using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class EhrRepository : Repository<EhrRecord>, IEhrRepository
    {
        public EhrRepository(HospitalDbContext context) : base(context)
        {
        }

        public List<EhrRecord> GetEhrForPatient(int patientId)
        {
            return Find(e => e.PatientId == patientId).ToList();
        }

        public EhrRecord CreateEhr(EhrRecord record)
        {
            record.VisitDate = DateTime.Now;
            Add(record);
            SaveChanges();
            return record;
        }
    }
}
