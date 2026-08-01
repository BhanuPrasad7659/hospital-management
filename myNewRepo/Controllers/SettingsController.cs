using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        [Route("settings")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("settings/save-hospital")]
        public IActionResult SaveHospital(string hospitalName, string tagline, string contact, string email, string address)
        {
            TempData["SuccessMessage"] = "Hospital settings updated successfully (in-memory demo)!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("settings/change-password")]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New passwords do not match.";
                return RedirectToAction("Index");
            }
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index");
        }
    }
}
