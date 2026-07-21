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

        // GET: api/users/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetUser(int id)
        {
            var user = _hospitalService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");
            return Ok(user.ToDto());
        }

        // GET: api/users/by-username/{username}
        [HttpGet("by-username/{username}")]
        public IActionResult GetUserByUsername(string username)
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
            
            var createdUser = _hospitalService.GetUsers().FirstOrDefault(u => u.Username.Equals(requestDto.Username, System.StringComparison.OrdinalIgnoreCase));
            
            if (createdUser != null && requestDto.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _hospitalService.UpdateDoctorProfile(
                    createdUser.Id, 
                    requestDto.Specialty ?? "", 
                    requestDto.Biography ?? "", 
                    requestDto.ContactNumber ?? "", 
                    requestDto.Email ?? ""
                );
            }

            return CreatedAtAction(nameof(GetUser), new { id = createdUser?.Id ?? 0 }, createdUser?.ToDto());
        }

        // PUT: api/users/{id}
        [HttpPut("{id:int}")]
        public IActionResult UpdateDoctorProfile(int id, [FromBody] UserUpdateDto requestDto)
        {
            var user = _hospitalService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");

            if (user.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _hospitalService.UpdateDoctorProfile(
                    id, 
                    requestDto.Specialty ?? "", 
                    requestDto.Biography ?? "", 
                    requestDto.ContactNumber ?? "", 
                    requestDto.Email ?? ""
                );
                _hospitalService.AssignStaff(
                    !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                    user.Username, 
                    user.Role
                );
                
                var updatedDoctor = _hospitalService.GetUserById(id);
                return Ok(updatedDoctor?.ToDto());
            }

            _hospitalService.AssignStaff(
                !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                user.Username, 
                !string.IsNullOrEmpty(requestDto.Role) ? requestDto.Role : user.Role
            );
            
            var updatedUser = _hospitalService.GetUserById(id);
            return Ok(updatedUser?.ToDto());
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _hospitalService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");

            _hospitalService.DeleteUser(id);
            return NoContent();
        }
    }
}
