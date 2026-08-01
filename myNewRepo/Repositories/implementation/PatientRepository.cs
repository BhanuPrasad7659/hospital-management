using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        private readonly HospitalDbContext _context;

        public PatientRepository(HospitalDbContext context) : base(context)
        {
            _context = context;
        }

        public Patient RegisterPatient(Patient patient)
        {
            patient.Status = "REGISTERED";
            patient.CreatedDate = DateTime.Now;
            Add(patient);
            SaveChanges();
            return patient;
        }

        public void UpdatePatient(Patient patient)
        {
            var existing = GetById(patient.PatientId);
            if (existing != null)
            {
                existing.Name = patient.Name;
                existing.Age = patient.Age;
                existing.Gender = patient.Gender;
                existing.Address = patient.Address;
                existing.ContactNumber = patient.ContactNumber;
                existing.Status = patient.Status;

                existing.Ward = patient.Ward;
                existing.BedNumber = patient.BedNumber;

                Update(existing);
                SaveChanges();
            }
        }

        public void DeletePatient(int patientId)
        {
            var patient = GetById(patientId);
            if (patient != null)
            {
                Delete(patient);
                SaveChanges();
            }
        }

        public void AssignDoctorToPatient(int patientId, int doctorId, string doctorName)
        {
            var patient = GetById(patientId);
            if (patient != null)
            {
                if (doctorId > 0 && string.IsNullOrWhiteSpace(doctorName))
                {
                    var doc = _context.Users.FirstOrDefault(u => u.Id == doctorId);
                    if (doc != null) doctorName = doc.FullName;
                }

                patient.AssignedDoctorId = doctorId;
                patient.AssignedDoctorName = doctorName ?? "";
                Update(patient);
                SaveChanges();
            }
        }

        public void UpdateStatus(int patientId, string status)
        {
            var patient = GetById(patientId);
            if (patient != null)
            {
                patient.Status = status;
                Update(patient);
                SaveChanges();
            }
        }

        public void AssignBedToPatient(int patientId, string ward, string bedNumber)
        {
            var patient = GetById(patientId);
            if (patient != null)
            {
                patient.Ward = ward;
                patient.BedNumber = bedNumber;

                Update(patient);
                SaveChanges();
            }
        }
    }
}
