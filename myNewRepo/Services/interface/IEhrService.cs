using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IEhrService
    {
        List<EhrRecord> GetEhrRecords();
        List<EhrRecord> GetEhrForPatient(int patientId);
        EhrRecord CreateEhr(EhrRecord record);
    }
}
