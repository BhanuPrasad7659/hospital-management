using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;

namespace CogMediHospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If already logged in, we can optionally redirect to their respective dashboard, 
            // but let's keep the landing page accessible.
            return View();
        }

        public IActionResult Roles()
        {
            return Redirect("/#portals");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
