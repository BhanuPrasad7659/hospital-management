using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly HospitalService _hospitalService;

        public AuthController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // --- ADMIN LOGIN ---
        [HttpGet]
        [Route("login/admin")]
        public IActionResult AdminLogin()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("admin"))
                return RedirectToAction("Dashboard", "Admin");

            return View(new LoginViewModel { Role = "admin" });
        }

        [HttpPost]
        [Route("login/admin")]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "admin");
            return RedirectToAction("Dashboard", "Admin");
        }

        // --- RECEPTIONIST LOGIN ---
        [HttpGet]
        [Route("login/receptionist")]
        public IActionResult ReceptionistLogin()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("receptionist"))
                return RedirectToAction("Dashboard", "Receptionist");

            return View(new LoginViewModel { Role = "receptionist" });
        }

        [HttpPost]
        [Route("login/receptionist")]
        public async Task<IActionResult> ReceptionistLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "receptionist");
            return RedirectToAction("Dashboard", "Receptionist");
        }

        // --- DOCTOR LOGIN ---
        [HttpGet]
        [Route("login/doctor")]
        public IActionResult DoctorLogin()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("doctor"))
                return RedirectToAction("Dashboard", "Doctor");

            return View(new LoginViewModel { Role = "doctor" });
        }

        [HttpPost]
        [Route("login/doctor")]
        public async Task<IActionResult> DoctorLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "doctor");
            return RedirectToAction("Dashboard", "Doctor");
        }

        // --- LAB TECHNICIAN LOGIN ---
        [HttpGet]
        [Route("login/lab")]
        [Route("login/laboratory")]
        public IActionResult LabLogin()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("laboratory"))
                return RedirectToAction("Dashboard", "Lab");

            return View(new LoginViewModel { Role = "laboratory" });
        }

        [HttpPost]
        [Route("login/lab")]
        [Route("login/laboratory")]
        public async Task<IActionResult> LabLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "laboratory");
            return RedirectToAction("Dashboard", "Lab");
        }

        // --- PHARMACIST LOGIN ---
        [HttpGet]
        [Route("login/pharmacist")]
        [Route("login/pharmiacist")]
        public IActionResult PharmacistLogin()
        {
            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("pharmacist") || User.IsInRole("pharmiacist")))
                return RedirectToAction("Dashboard", "Pharmacist");

            return View(new LoginViewModel { Role = "pharmiacist" });
        }

        [HttpPost]
        [Route("login/pharmacist")]
        [Route("login/pharmiacist")]
        public async Task<IActionResult> PharmacistLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "pharmiacist");
            return RedirectToAction("Dashboard", "Pharmacist");
        }

        // --- BILLING OFFICER LOGIN ---
        [HttpGet]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public IActionResult BillingLogin()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("billing discharge"))
                return RedirectToAction("Dashboard", "Billing");

            return View(new LoginViewModel { Role = "billing discharge" });
        }

        [HttpPost]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public async Task<IActionResult> BillingLogin(LoginViewModel model)
        {
            await SignInUser(model.Username, "billing discharge");
            return RedirectToAction("Dashboard", "Billing");
        }

        // --- LOGOUT ---
        [HttpGet]
        [Route("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Roles", "Home");
        }

        // Helper to sign in user via cookies (Password-less, auto-filling defaults)
        private async Task<bool> SignInUser(string username, string expectedRole)
        {
            // If username is empty, default it to the role name
            if (string.IsNullOrWhiteSpace(username))
            {
                username = expectedRole;
            }

            var user = _hospitalService.ValidateUser(username, expectedRole);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.GivenName, user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return true;
            }
            return false;
        }
    }
}
