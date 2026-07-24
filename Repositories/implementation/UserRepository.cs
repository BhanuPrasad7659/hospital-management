using CogMediHospitalManagementSystem.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories.implementation
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(HospitalDbContext context) : base(context)
        {
        }

        public User? ValidateUser(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = role;
            }

            // FIXED: Using .ToLower() == .ToLower() for EF Core translation
            var user = Get(u => u.Username.ToLower() == username.ToLower() && u.Role.ToLower() == role.ToLower());
            if (user == null)
            {
                string defaultName = username;
                // In-memory C# comparisons are fine here
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase)) defaultName = "System Administrator";
                else if (username.Equals("receptionist", StringComparison.OrdinalIgnoreCase)) defaultName = "Receptionist Staff";
                else if (username.Equals("doctor", StringComparison.OrdinalIgnoreCase) || username.StartsWith("doctor", StringComparison.OrdinalIgnoreCase)) defaultName = "Dr. Ramesh";
                else if (username.Equals("laboratory", StringComparison.OrdinalIgnoreCase) || username.Equals("lab", StringComparison.OrdinalIgnoreCase)) defaultName = "Diagnostic Lab Technician";
                else if (username.Equals("pharmiacist", StringComparison.OrdinalIgnoreCase) || username.Equals("pharmacist", StringComparison.OrdinalIgnoreCase)) defaultName = "Chief Pharmacist";
                else if (username.Equals("billing discharge", StringComparison.OrdinalIgnoreCase) || username.Equals("billing", StringComparison.OrdinalIgnoreCase)) defaultName = "Billing Officer";

                user = new User { Username = username, Password = password, Role = role, FullName = defaultName };
                Add(user);
                SaveChanges();
            }
            else
            {
                if (user.Password != password)
                {
                    return null;
                }
            }
            return user;
        }

        public List<User> GetDoctors()
        {
            // FIXED
            return Find(u => u.Role.ToLower() == "doctor").ToList();
        }

        public User? GetDoctorById(int doctorId)
        {
            // FIXED
            return Get(u => u.Id == doctorId && u.Role.ToLower() == "doctor");
        }

        public void AssignStaff(string fullName, string username, string role, string password)
        {
            // FIXED
            var existing = Get(u => u.Username.ToLower() == username.ToLower());
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Role = role;
                existing.Password = password;
                Update(existing);
            }
            else
            {
                Add(new User { FullName = fullName, Username = username, Role = role, Password = password });
            }
            SaveChanges();
        }

        public void UpdateDoctorProfile(int doctorId, string specialty, string biography, string contactNumber, string email)
        {
            var doc = GetDoctorById(doctorId);
            if (doc != null)
            {
                doc.Specialty = specialty;
                doc.Biography = biography;
                doc.ContactNumber = contactNumber;
                doc.Email = email;
                Update(doc);
                SaveChanges();
            }
        }

        public void UpdateDoctorProfile(string username, string specialty, string biography, string contactNumber, string email)
        {
            // FIXED
            var doc = Get(u => u.Username.ToLower() == username.ToLower() && u.Role.ToLower() == "doctor");
            if (doc != null)
            {
                doc.Specialty = specialty;
                doc.Biography = biography;
                doc.ContactNumber = contactNumber;
                doc.Email = email;
                Update(doc);
                SaveChanges();
            }
        }

        public void UpdateUserProfile(string username, string fullName, string contactNumber, string email)
        {
            // FIXED
            var user = Get(u => u.Username.ToLower() == username.ToLower());
            if (user != null)
            {
                user.FullName = fullName;
                user.ContactNumber = contactNumber;
                user.Email = email;
                Update(user);
                SaveChanges();
            }
        }
    }
}