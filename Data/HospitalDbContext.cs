using Microsoft.EntityFrameworkCore;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Admission> Admissions { get; set; } = null!;
        public DbSet<EhrRecord> EhrRecords { get; set; } = null!;
        public DbSet<LabOrder> LabOrders { get; set; } = null!;
        public DbSet<TreatmentPlan> TreatmentPlans { get; set; } = null!;
        public DbSet<PharmacyRecord> PharmacyRecords { get; set; } = null!;
        public DbSet<BillingRecord> BillingRecords { get; set; } = null!;
        public DbSet<DischargeRecord> DischargeRecords { get; set; } = null!;
        public DbSet<MedicineStock> MedicineStocks { get; set; } = null!;
    }
}
