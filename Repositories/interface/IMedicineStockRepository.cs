using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IMedicineStockRepository : IRepository<MedicineStock>
    {
        Dictionary<string, int> GetStockDictionary();
        void UpdateStock(string medicineName, int amount);
    }
}
