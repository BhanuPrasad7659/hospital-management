using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IEhrRepository : IRepository<EhrRecord>
    {
        List<EhrRecord> GetEhrForPatient(int patientId);
        EhrRecord CreateEhr(EhrRecord record);
    }
}
