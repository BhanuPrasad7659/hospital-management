using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class PharmacyRepository : Repository<PharmacyRecord>, IPharmacyRepository
    {
        private readonly HospitalDbContext _context;

        public PharmacyRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public List<PharmacyRecord> GetRecordsForPatient(int patientId)
        {
            return Find(p => p.PatientId == patientId).ToList();
        }

        public PharmacyRecord CreateRecord(PharmacyRecord record)
        {
            record.Status = "PENDING";
            Add(record);
            SaveChanges();
            return record;
        }

        public void DispenseMedicine(int recordId, string pharmacistName)
        {
            var record = GetById(recordId);
            if (record != null)
            {
                record.Status = "DISPENSED";
                record.DispensedDate = DateTime.Now;
                record.PharmacistName = pharmacistName;
                Update(record);

                var stock = _context.MedicineStocks.FirstOrDefault(m => m.MedicineName.ToLower() == record.MedicineName.ToLower());
                if (stock != null)
                {
                    stock.Quantity = Math.Max(0, stock.Quantity - record.Quantity);
                    _context.MedicineStocks.Update(stock);
                }

                SaveChanges();
            }
        }

        public void DeleteRecord(int recordId)
        {
            var record = GetById(recordId);
            if (record != null)
            {
                Delete(record);
                SaveChanges();
            }
        }
    }
}
