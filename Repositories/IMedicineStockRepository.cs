using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IMedicineStockRepository : IRepository<MedicineStock>
    {
        Dictionary<string, int> GetStockDictionary();
        void UpdateStock(string medicineName, int amount);
    }
}
