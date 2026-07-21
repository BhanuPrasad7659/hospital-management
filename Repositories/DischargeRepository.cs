using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class DischargeRepository : Repository<DischargeRecord>, IDischargeRepository
    {
        public DischargeRepository(HospitalDbContext context) : base(context)
        {
        }

        public DischargeRecord? GetForPatient(int patientId)
        {
            return Get(d => d.PatientId == patientId);
        }

        public List<DischargeRecord> GetDischargeHistory()
        {
            return GetAll().OrderByDescending(d => d.DischargeDate).ToList();
        }
    }
}
