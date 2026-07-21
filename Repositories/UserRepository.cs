using System;
using System.Collections.Generic;
using System.Linq;
using CogMediHospitalManagementSystem.Data;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(HospitalDbContext context) : base(context)
        {
        }

        public User ValidateUser(string username, string role)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = role;
            }

            var user = Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                string defaultName = username;
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase)) defaultName = "System Administrator";
                else if (username.Equals("receptionist", StringComparison.OrdinalIgnoreCase)) defaultName = "Receptionist Staff";
                else if (username.Equals("doctor", StringComparison.OrdinalIgnoreCase) || username.StartsWith("doctor", StringComparison.OrdinalIgnoreCase)) defaultName = "Dr. Ramesh";
                else if (username.Equals("laboratory", StringComparison.OrdinalIgnoreCase) || username.Equals("lab", StringComparison.OrdinalIgnoreCase)) defaultName = "Diagnostic Lab Technician";
                else if (username.Equals("pharmiacist", StringComparison.OrdinalIgnoreCase) || username.Equals("pharmacist", StringComparison.OrdinalIgnoreCase)) defaultName = "Chief Pharmacist";
                else if (username.Equals("billing discharge", StringComparison.OrdinalIgnoreCase) || username.Equals("billing", StringComparison.OrdinalIgnoreCase)) defaultName = "Billing Officer";

                user = new User { Username = username, Role = role, FullName = defaultName };
                Add(user);
                SaveChanges();
            }
            return user;
        }

        public List<User> GetDoctors()
        {
            return Find(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public User? GetDoctorById(int doctorId)
        {
            return Get(u => u.Id == doctorId && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
        }

        public void AssignStaff(string fullName, string username, string role)
        {
            var existing = Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Role = role;
                Update(existing);
            }
            else
            {
                Add(new User { FullName = fullName, Username = username, Role = role });
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
            var doc = Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
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
            var user = Get(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
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
