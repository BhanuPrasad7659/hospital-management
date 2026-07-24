using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
                private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // --- ADMIN LOGIN ---
        [HttpGet]
        [Route("login/admin")]
        public IActionResult AdminLogin()
        {
            return View(new LoginViewModel { Role = "admin" });
        }

        [HttpPost]
        [Route("login/admin")]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            var success = await SignInUser(model.Username, model.Password, "admin");
            if (success)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- ADMIN REGISTER ---
        [HttpGet]
        [Route("register/admin")]
        public IActionResult AdminRegister()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Route("register/admin")]
        public IActionResult AdminRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username is taken
            var existing = _userService.GetUsers().FirstOrDefault(u => u.Username.ToLower() == model.Username.ToLower());
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                return View(model);
            }

            // Create new admin user in the system
            _userService.AssignStaff(model.FullName, model.Username, "admin", model.Password);

            TempData["Success"] = "Administrator account registered successfully! You can now log in.";
            return RedirectToAction(nameof(AdminLogin));
        }

        // --- RECEPTIONIST LOGIN ---
        [HttpGet]
        [Route("login/receptionist")]
        public IActionResult ReceptionistLogin()
        {
            var receptionists = _userService.GetUsers().Where(u => u.Role.Equals("receptionist", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Receptionists = receptionists;
            return View(new LoginViewModel { Role = "receptionist" });
        }

        [HttpPost]
        [Route("login/receptionist")]
        public async Task<IActionResult> ReceptionistLogin(LoginViewModel model)
        {
            var username = string.IsNullOrWhiteSpace(model.Username) ? "receptionist" : model.Username;
            var success = await SignInUser(username, model.Password, "receptionist");
            if (success)
            {
                return RedirectToAction("Dashboard", "Receptionist");
            }
            var receptionists = _userService.GetUsers().Where(u => u.Role.Equals("receptionist", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Receptionists = receptionists;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- DOCTOR LOGIN ---
        [HttpGet]
        [Route("login/doctor")]
        public IActionResult DoctorLogin()
        {
            var doctors = _userService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Doctors = doctors;
            return View(new LoginViewModel { Role = "doctor" });
        }

        [HttpPost]
        [Route("login/doctor")]
        public async Task<IActionResult> DoctorLogin(LoginViewModel model)
        {
            var doctorUsername = string.IsNullOrWhiteSpace(model.Username) ? "Doctor Ramesh" : model.Username;
            var success = await SignInUser(doctorUsername, model.Password, "doctor");
            if (success)
            {
                return RedirectToAction("Dashboard", "Doctor");
            }
            var doctors = _userService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Doctors = doctors;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- LAB TECHNICIAN LOGIN ---
        [HttpGet]
        [Route("login/lab")]
        [Route("login/laboratory")]
        public IActionResult LabLogin()
        {
            return View(new LoginViewModel { Role = "laboratory" });
        }

        [HttpPost]
        [Route("login/lab")]
        [Route("login/laboratory")]
        public async Task<IActionResult> LabLogin(LoginViewModel model)
        {
            var success = await SignInUser(model.Username, model.Password, "laboratory");
            if (success)
            {
                return RedirectToAction("Dashboard", "Lab");
            }
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- PHARMACIST LOGIN ---
        [HttpGet]
        [Route("login/pharmacist")]
        [Route("login/pharmiacist")]
        public IActionResult PharmacistLogin()
        {
            return View(new LoginViewModel { Role = "pharmiacist" });
        }

        [HttpPost]
        [Route("login/pharmacist")]
        [Route("login/pharmiacist")]
        public async Task<IActionResult> PharmacistLogin(LoginViewModel model)
        {
            var success = await SignInUser(model.Username, model.Password, "pharmiacist");
            if (success)
            {
                return RedirectToAction("Dashboard", "Pharmacist");
            }
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- BILLING OFFICER LOGIN ---
        [HttpGet]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public IActionResult BillingLogin()
        {
            var billingOfficers = _userService.GetUsers().Where(u => u.Role.Equals("billing discharge", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.BillingOfficers = billingOfficers;
            return View(new LoginViewModel { Role = "billing discharge" });
        }

        [HttpPost]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public async Task<IActionResult> BillingLogin(LoginViewModel model)
        {
            var username = string.IsNullOrWhiteSpace(model.Username) ? "billing" : model.Username;
            var success = await SignInUser(username, model.Password, "billing discharge");
            if (success)
            {
                return RedirectToAction("Dashboard", "Billing");
            }
            var billingOfficers = _userService.GetUsers().Where(u => u.Role.Equals("billing discharge", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.BillingOfficers = billingOfficers;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // --- LOGOUT ---
        [HttpGet]
        [Route("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Roles", "Home");
        }

        // Helper to sign in user via cookies
        private async Task<bool> SignInUser(string username, string password, string expectedRole)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = expectedRole;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                password = "password";
            }

            var user = _userService.ValidateUser(username, password, expectedRole);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
