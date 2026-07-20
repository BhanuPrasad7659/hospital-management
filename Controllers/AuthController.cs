using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;

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
            var receptionists = _hospitalService.GetUsers().Where(u => u.Role.Equals("receptionist", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!receptionists.Any())
            {
                receptionists = new List<User>
                {
                    new User { Username = "receptionist", Role = "receptionist", FullName = "Sita Ramam (Front Desk Receptionist 1)" },
                    new User { Username = "receptionist2", Role = "receptionist", FullName = "Ananya Rao (Admissions Desk Receptionist 2)" }
                };
            }
            ViewBag.Receptionists = receptionists;
            return View(new LoginViewModel { Role = "receptionist" });
        }

        [HttpPost]
        [Route("login/receptionist")]
        public async Task<IActionResult> ReceptionistLogin(LoginViewModel model)
        {
            var username = string.IsNullOrWhiteSpace(model.Username) ? "receptionist" : model.Username;
            await SignInUser(username, "receptionist");
            return RedirectToAction("Dashboard", "Receptionist");
        }

        // --- DOCTOR LOGIN ---
        [HttpGet]
        [Route("login/doctor")]
        public IActionResult DoctorLogin()
        {
            var doctors = _hospitalService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!doctors.Any())
            {
                doctors = new List<User>
                {
                    new User { Username = "doctor1", Role = "doctor", FullName = "Dr. Harsha Vardhan", Specialty = "Cardiology" },
                    new User { Username = "doctor2", Role = "doctor", FullName = "Dr. Priya Sharma", Specialty = "Pediatrics" },
                    new User { Username = "doctor3", Role = "doctor", FullName = "Dr. Amit Verma", Specialty = "Orthopedics" },
                    new User { Username = "doctor4", Role = "doctor", FullName = "Dr. Shalini Gupta", Specialty = "Neurology" }
                };
            }
            ViewBag.Doctors = doctors;
            return View(new LoginViewModel { Role = "doctor" });
        }

        [HttpPost]
        [Route("login/doctor")]
        public async Task<IActionResult> DoctorLogin(LoginViewModel model)
        {
            var doctorUsername = string.IsNullOrWhiteSpace(model.Username) ? "doctor1" : model.Username;
            await SignInUser(doctorUsername, "doctor");
            return RedirectToAction("Dashboard", "Doctor");
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
            await SignInUser(model.Username, "laboratory");
            return RedirectToAction("Dashboard", "Lab");
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
            await SignInUser(model.Username, "pharmiacist");
            return RedirectToAction("Dashboard", "Pharmacist");
        }

        // --- BILLING OFFICER LOGIN ---
        [HttpGet]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public IActionResult BillingLogin()
        {
            var billingOfficers = _hospitalService.GetUsers().Where(u => u.Role.Equals("billing discharge", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!billingOfficers.Any())
            {
                billingOfficers = new List<User>
                {
                    new User { Username = "billing", Role = "billing discharge", FullName = "Ramesh Varma (Senior Billing Officer 1)" },
                    new User { Username = "billing2", Role = "billing discharge", FullName = "Kavita Reddy (Discharge Clearance Officer 2)" }
                };
            }
            ViewBag.BillingOfficers = billingOfficers;
            return View(new LoginViewModel { Role = "billing discharge" });
        }

        [HttpPost]
        [Route("login/billing")]
        [Route("login/billing-discharge")]
        public async Task<IActionResult> BillingLogin(LoginViewModel model)
        {
            var username = string.IsNullOrWhiteSpace(model.Username) ? "billing" : model.Username;
            await SignInUser(username, "billing discharge");
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
