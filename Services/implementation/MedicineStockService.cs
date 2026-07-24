using System.Collections.Generic;
using CogMediHospitalManagementSystem.Repositories.Interfaces;
using CogMediHospitalManagementSystem.Services.Interfaces;

namespace CogMediHospitalManagementSystem.Services.implementation
{
    public class MedicineStockService : IMedicineStockService
    {
        private readonly IMedicineStockRepository _medicineStockRepository;

        public MedicineStockService(IMedicineStockRepository medicineStockRepository)
        {
            _medicineStockRepository = medicineStockRepository;
        }

        public Dictionary<string, int> MedicineStock => _medicineStockRepository.GetStockDictionary();

        public void UpdateMedicineStock(string medicineName, int amount)
            => _medicineStockRepository.UpdateStock(medicineName, amount);
    }
}
