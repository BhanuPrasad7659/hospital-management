using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class AdmissionRepository : Repository<Admission>, IAdmissionRepository
    {
        private readonly HospitalDbContext _context;

        public AdmissionRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public Admission AdmitPatient(int patientId, string ward, string bedNumber, int doctorId, string doctorName)
        {
            if (doctorId > 0 && string.IsNullOrWhiteSpace(doctorName))
            {
                var doc = _context.Users.FirstOrDefault(u => u.Id == doctorId);
                if (doc != null) doctorName = doc.FullName;
            }

            var admission = new Admission
            {
                PatientId = patientId,
                AdmissionDate = DateTime.Now,
                Ward = ward,
                BedNumber = bedNumber,
                Status = "ADMITTED",
                AssignedDoctorId = doctorId,
                AssignedDoctorName = doctorName ?? ""
            };
            Add(admission);
            SaveChanges();
            return admission;
        }

        public void UpdateAdmission(Admission admission)
        {
            var existing = GetById(admission.AdmissionId);
            if (existing != null)
            {
                if (admission.AssignedDoctorId.HasValue && admission.AssignedDoctorId.Value > 0 && string.IsNullOrWhiteSpace(admission.AssignedDoctorName))
                {
                    var doc = _context.Users.FirstOrDefault(u => u.Id == admission.AssignedDoctorId.Value);
                    if (doc != null) admission.AssignedDoctorName = doc.FullName;
                }

                existing.Ward = admission.Ward;
                existing.BedNumber = admission.BedNumber;
                existing.Status = admission.Status;
                existing.DischargeDate = admission.DischargeDate;
                existing.AssignedDoctorId = admission.AssignedDoctorId;
                existing.AssignedDoctorName = admission.AssignedDoctorName;
                Update(existing);
                SaveChanges();
            }
        }

        public void DeleteAdmission(int admissionId)
        {
            var adm = GetById(admissionId);
            if (adm != null)
            {
                Delete(adm);
                SaveChanges();
            }
        }

        public void DischargeAdmission(int patientId)
        {
            var admission = Get(a => a.PatientId == patientId && a.Status == "ADMITTED");
            if (admission != null)
            {
                admission.Status = "DISCHARGED";
                admission.DischargeDate = DateTime.Now;
                Update(admission);
                SaveChanges();
            }
        }

        public void AssignDoctorToAdmission(int patientId, int doctorId, string doctorName)
        {
            var admission = Get(a => a.PatientId == patientId && a.Status == "ADMITTED");
            if (admission != null)
            {
                if (doctorId > 0 && string.IsNullOrWhiteSpace(doctorName))
                {
                    var doc = _context.Users.FirstOrDefault(u => u.Id == doctorId);
                    if (doc != null) doctorName = doc.FullName;
                }

                admission.AssignedDoctorId = doctorId;
                admission.AssignedDoctorName = doctorName ?? "";
                Update(admission);
                SaveChanges();
            }
        }

        public WardAnalyticsData GetWardAdmissionsData()
        {
            var wardGroups = GetAll()
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Ward) ? "General" : a.Ward)
                .Select(g => new { Ward = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            var labels = wardGroups.Select(w => w.Ward).ToList();
            var values = wardGroups.Select(w => w.Count).ToList();

            if (labels.Count == 0)
            {
                labels = new List<string> { "General Ward", "ICU Room", "Pediatrics", "Cardiac ICU" };
                values = new List<int> { 0, 0, 0, 0 };
            }

            return new WardAnalyticsData { Labels = labels, Values = values };
        }

        public List<WardSummaryItem> GetWardSummaries()
        {
            return GetAll()
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Ward) ? "General" : a.Ward)
                .Select(g => new WardSummaryItem
                {
                    WardName = g.Key,
                    ActiveAdmissions = g.Count(a => a.Status.Equals("ADMITTED", StringComparison.OrdinalIgnoreCase)),
                    TotalDischarged = g.Count(a => a.Status.Equals("DISCHARGED", StringComparison.OrdinalIgnoreCase)),
                    TotalAdmissions = g.Count()
                })
                .OrderByDescending(w => w.ActiveAdmissions)
                .ToList();
        }
    }
}
