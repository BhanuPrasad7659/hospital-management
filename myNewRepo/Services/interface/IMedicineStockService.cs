using System.Collections.Generic;

namespace CogMediHospitalManagementSystem.Services.Interfaces
{
    public interface IMedicineStockService
    {
        Dictionary<string, int> MedicineStock { get; }
        Dictionary<string, decimal> MedicinePrices { get; }
        void UpdateMedicineStock(string medicineName, int amount);
    }
}
