using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsApiController : ControllerBase
    {
        [HttpPost("save-hospital")]
        public IActionResult SaveHospital([FromBody] HospitalSettingsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.HospitalName))
            {
                return BadRequest("HospitalName is required.");
            }
            return Ok(new { Message = "Hospital settings updated successfully (in-memory demo)!", Data = request });
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("CurrentPassword and NewPassword are required.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("New passwords do not match.");
            }

            return Ok(new { Message = "Password changed successfully!" });
        }

        public class HospitalSettingsRequest
        {
            public string HospitalName { get; set; } = string.Empty;
            public string Tagline { get; set; } = string.Empty;
            public string Contact { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
