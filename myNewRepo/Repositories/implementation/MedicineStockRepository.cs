using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class MedicineStockRepository : Repository<MedicineStock>, IMedicineStockRepository
    {
        public MedicineStockRepository(HospitalDbContext context) : base(context)
        {
        }

        public Dictionary<string, int> GetStockDictionary()
        {
            // Groups by MedicineName and sums the quantities of any duplicate records
            return GetAll()
                .GroupBy(m => m.MedicineName)
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Quantity));
        }

        public Dictionary<string, decimal> GetPriceDictionary()
        {
            // Groups by MedicineName and takes the price from the first record to prevent duplicate key crashes
            return GetAll()
                .GroupBy(m => m.MedicineName)
                .ToDictionary(g => g.Key, g => g.First().Price);
        }

        public void UpdateStock(string medicineName, int amount)
        {
            var item = Get(m => m.MedicineName.ToLower() == medicineName.ToLower());
            if (item != null)
            {
                item.Quantity += amount;
                if (item.Quantity < 0) item.Quantity = 0;
                Update(item);
            }
            else
            {
                if (amount > 0)
                {
                    Add(new MedicineStock { MedicineName = medicineName, Quantity = amount });
                }
            }
            SaveChanges();
        }
    }
}