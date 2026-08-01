using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IDischargeService
    {
        void DischargePatient(int patientId, string remarks);
        List<DischargeRecord> GetDischargeRecords();
    }
}
