using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class MedicineStockRepository : Repository<MedicineStock>, IMedicineStockRepository
    {
        public MedicineStockRepository(HospitalDbContext context) : base(context)
        {
        }

        public Dictionary<string, int> GetStockDictionary()
        {
            return GetAll().ToDictionary(m => m.MedicineName, m => m.Quantity);
        }

        public void UpdateStock(string medicineName, int amount)
        {
            var item = Get(m => m.MedicineName.Equals(medicineName, StringComparison.OrdinalIgnoreCase));
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
