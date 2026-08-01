using System.Collections.Generic;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.Interfaces
{
    public interface IMedicineStockRepository : IRepository<MedicineStock>
    {
        Dictionary<string, int> GetStockDictionary();
        Dictionary<string, decimal> GetPriceDictionary();
        void UpdateStock(string medicineName, int amount);
    }
}
