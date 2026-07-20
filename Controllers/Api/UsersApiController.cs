using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public UsersApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _hospitalService.GetUsers();
            var dtos = users.Select(u => u.ToDto());
            return Ok(dtos);
        }

        // GET: api/users/{username}
        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            var user = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            if (user == null) return NotFound($"User '{username}' not found.");
            return Ok(user.ToDto());
        }

        // POST: api/users
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserCreateDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request body is missing.");
            }

            _hospitalService.AssignStaff(requestDto.FullName, requestDto.Username, requestDto.Role);
            
            // If details are provided for a doctor, update profile details too
            if (requestDto.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _hospitalService.UpdateDoctorProfile(
                    requestDto.Username, 
                    requestDto.Specialty ?? "", 
                    requestDto.Biography ?? "", 
                    requestDto.ContactNumber ?? "", 
                    requestDto.Email ?? ""
                );
            }

            var createdUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(requestDto.Username, System.StringComparison.OrdinalIgnoreCase));
            return CreatedAtAction(nameof(GetUser), new { username = requestDto.Username }, createdUser?.ToDto());
        }

        // PUT: api/users/{username}
        [HttpPut("{username}")]
        public IActionResult UpdateDoctorProfile(string username, [FromBody] UserUpdateDto requestDto)
        {
            var user = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            if (user == null) return NotFound($"User '{username}' not found.");

            if (user.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _hospitalService.UpdateDoctorProfile(
                    username, 
                    requestDto.Specialty ?? "", 
                    requestDto.Biography ?? "", 
                    requestDto.ContactNumber ?? "", 
                    requestDto.Email ?? ""
                );
                _hospitalService.AssignStaff(
                    !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                    username, 
                    user.Role
                );
                
                var updatedDoctor = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
                return Ok(updatedDoctor?.ToDto());
            }

            // For other roles, just update display name and role if provided
            _hospitalService.AssignStaff(
                !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                username, 
                !string.IsNullOrEmpty(requestDto.Role) ? requestDto.Role : user.Role
            );
            
            var updatedUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            return Ok(updatedUser?.ToDto());
        }

        // DELETE: api/users/{username}
        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            var user = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            if (user == null) return NotFound($"User '{username}' not found.");

            _hospitalService.DeleteUser(username);
            return NoContent();
        }
    }
}
