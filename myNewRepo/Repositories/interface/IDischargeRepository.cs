using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IDischargeRepository : IRepository<DischargeRecord>
    {
        DischargeRecord? GetForPatient(int patientId);
        List<DischargeRecord> GetDischargeHistory();
    }
}
