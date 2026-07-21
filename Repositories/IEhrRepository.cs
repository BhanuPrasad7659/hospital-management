using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IEhrRepository : IRepository<EhrRecord>
    {
        List<EhrRecord> GetEhrForPatient(int patientId);
        EhrRecord CreateEhr(EhrRecord record);
    }
}
