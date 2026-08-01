using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

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

        [HttpGet]
        [Route("register/admin")]
        public IActionResult AdminRegister()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Route("register/admin")]
        public async Task<IActionResult> AdminRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using (var client = new HttpClient(handler))
            {
                if (Request.Headers.TryGetValue("Cookie", out var cookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie.ToString());
                }

                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiBaseUrl = config["ApiSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var payload = new { FullName = model.FullName, Username = model.Username, Password = model.Password };
                try
                {
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl.TrimEnd('/')}/api/auth/register", payload);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Administrator account registered successfully! You can now log in.";
                        return RedirectToAction(nameof(AdminLogin));
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Registration failed.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error connecting to API: {ex.Message}");
                }
            }

            return View(model);
        }

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
            var success = await SignInUser(model.Username, model.Password, "receptionist");
            if (success)
            {
                return RedirectToAction("Dashboard", "Receptionist");
            }
            var receptionists = _userService.GetUsers().Where(u => u.Role.Equals("receptionist", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Receptionists = receptionists;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

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
            var success = await SignInUser(model.Username, model.Password, "doctor");
            if (success)
            {
                return RedirectToAction("Dashboard", "Doctor");
            }
            var doctors = _userService.GetUsers().Where(u => u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Doctors = doctors;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        [HttpGet]
        [Route("login/lab")]
        [Route("login/laboratory")]
        public IActionResult LabLogin()
        {
            var technicians = _userService.GetUsers().Where(u => u.Role == "laboratory").ToList();
            ViewBag.LabTechnicians = technicians;
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

        [HttpGet]
        [Route("login/pharmacist")]
        [Route("login/pharmiacist")]
        public IActionResult PharmacistLogin()
        {
            var pharmacists = _userService.GetUsers().Where(u => u.Role == "pharmacist" || u.Role == "pharmiacist").ToList();
            ViewBag.Pharmacists = pharmacists;
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
            var success = await SignInUser(model.Username, model.Password, "billing discharge");
            if (success)
            {
                return RedirectToAction("Dashboard", "Billing");
            }
            var billingOfficers = _userService.GetUsers().Where(u => u.Role.Equals("billing discharge", StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.BillingOfficers = billingOfficers;
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        [HttpGet]
        [Route("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Roles", "Home");
        }

        private async Task<bool> SignInUser(string username, string password, string expectedRole)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using (var client = new HttpClient(handler))
            {
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiBaseUrl = config["ApiSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var payload = new { Username = username, Password = password };
                try
                {
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl.TrimEnd('/')}/api/auth/login", payload);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                        
                        bool roleMatches = false;
                        if (result != null)
                        {
                            if (result.Role.Equals(expectedRole, StringComparison.OrdinalIgnoreCase))
                            {
                                roleMatches = true;
                            }
                            else if ((expectedRole.Equals("pharmacist", StringComparison.OrdinalIgnoreCase) || expectedRole.Equals("pharmiacist", StringComparison.OrdinalIgnoreCase)) &&
                                     (result.Role.Equals("pharmacist", StringComparison.OrdinalIgnoreCase) || result.Role.Equals("pharmiacist", StringComparison.OrdinalIgnoreCase)))
                            {
                                roleMatches = true;
                            }
                        }

                        if (roleMatches)
                        {
                            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                            {
                                foreach (var c in cookies)
                                {
                                    Response.Headers.Append("Set-Cookie", c);
                                }
                            }
                            return true;
                        }
                    }
                }
                catch
                {
                }
            }
            return false;
        }

        private class LoginResponse
        {
            public string Message { get; set; } = string.Empty;
            public string User { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
