using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
                private readonly IUserService _userService;

        public UsersApiController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            var dtos = users.Select(u => u.ToDto());
            return Ok(dtos);
        }

        // GET: api/users/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");
            return Ok(user.ToDto());
        }

        // GET: api/users/by-username/{username}
        [HttpGet("by-username/{username}")]
        public IActionResult GetUserByUsername(string username)
        {
            var user = _userService.GetUsers().FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
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

            _userService.AssignStaff(requestDto.FullName, requestDto.Username, requestDto.Role, requestDto.Password ?? "password");
            
            var createdUser = _userService.GetUsers().FirstOrDefault(u => u.Username.Equals(requestDto.Username, System.StringComparison.OrdinalIgnoreCase));
            
            if (createdUser != null && requestDto.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _userService.UpdateDoctorProfile(
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
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");

            if (user.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase))
            {
                _userService.UpdateDoctorProfile(
                    id, 
                    requestDto.Specialty ?? "", 
                    requestDto.Biography ?? "", 
                    requestDto.ContactNumber ?? "", 
                    requestDto.Email ?? ""
                );
                _userService.AssignStaff(
                    !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                    user.Username, 
                    user.Role,
                    !string.IsNullOrEmpty(requestDto.Password) ? requestDto.Password : user.Password
                );
                
                var updatedDoctor = _userService.GetUserById(id);
                return Ok(updatedDoctor?.ToDto());
            }

            _userService.AssignStaff(
                !string.IsNullOrEmpty(requestDto.FullName) ? requestDto.FullName : user.FullName, 
                user.Username, 
                !string.IsNullOrEmpty(requestDto.Role) ? requestDto.Role : user.Role,
                !string.IsNullOrEmpty(requestDto.Password) ? requestDto.Password : user.Password
            );
            
            var updatedUser = _userService.GetUserById(id);
            return Ok(updatedUser?.ToDto());
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");

            _userService.DeleteUser(id);
            return NoContent();
        }
    }
}
