using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.DTOs;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin,receptionist")]
    public class AdminApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            var dtos = users.Select(u => u.ToDto());
            return Ok(dtos);
        }

        [HttpGet("users/{id:int}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");
            return Ok(user.ToDto());
        }

        [HttpPost("users")]
        [Authorize(Roles = "admin")]
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

        [HttpPut("users/{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateUser(int id, [FromBody] UserUpdateDto requestDto)
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

        [HttpDelete("users/{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User #{id} not found.");

            _userService.DeleteUser(id);
            return NoContent();
        }
    }
}
