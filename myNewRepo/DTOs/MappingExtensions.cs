using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.DTOs
{
    public static class MappingExtensions
    {
        public static PatientDto ToDto(this Patient patient)
        {
            if (patient == null) return null!;
            return new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Age = patient.Age,
                Gender = patient.Gender,
                Address = patient.Address,
                ContactNumber = patient.ContactNumber,
                Status = patient.Status,
                CreatedDate = patient.CreatedDate,
                AssignedDoctorId = patient.AssignedDoctorId,
                AssignedDoctorName = patient.AssignedDoctorName,
                Ward = patient.Ward,
                BedNumber = patient.BedNumber
            };
        }

        public static Patient ToEntity(this PatientCreateDto dto)
        {
            if (dto == null) return null!;
            return new Patient
            {
                Name = dto.Name,
                Age = dto.Age,
                Gender = dto.Gender,
                Address = dto.Address,
                ContactNumber = dto.ContactNumber
            };
        }

        public static void UpdateEntity(this PatientUpdateDto dto, Patient patient)
        {
            if (dto == null || patient == null) return;
            patient.Name = dto.Name;
            patient.Age = dto.Age;
            patient.Gender = dto.Gender;
            patient.Address = dto.Address;
            patient.ContactNumber = dto.ContactNumber;
            patient.Status = dto.Status;
        }

        public static UserDto ToDto(this User user)
        {
            if (user == null) return null!;
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                FullName = user.FullName,
                Specialty = user.Specialty,
                Biography = user.Biography,
                ContactNumber = user.ContactNumber,
                Email = user.Email
            };
        }

        public static User ToEntity(this UserCreateDto dto)
        {
            if (dto == null) return null!;
            return new User
            {
                Username = dto.Username,
                Role = dto.Role,
                FullName = dto.FullName,
                Specialty = dto.Specialty,
                Biography = dto.Biography,
                ContactNumber = dto.ContactNumber,
                Email = dto.Email
            };
        }
    }
}
