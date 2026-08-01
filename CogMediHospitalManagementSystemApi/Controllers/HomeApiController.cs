using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/home")]
    [Authorize]
    public class HomeApiController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly IAdmissionService _admissionService;
        private readonly IBillingService _billingService;

        public HomeApiController(
            IPatientService patientService,
            IUserService userService,
            IAdmissionService admissionService,
            IBillingService billingService)
        {
            _patientService = patientService;
            _userService = userService;
            _admissionService = admissionService;
            _billingService = billingService;
        }

        [HttpGet("stats")]
        public IActionResult GetHospitalStats()
        {
            var patientsCount = _patientService.GetPatients().Count;
            var activeAdmissions = _admissionService.GetAdmissions().Count(a => a.Status.Equals("ADMITTED", System.StringComparison.OrdinalIgnoreCase));
            var staffCount = _userService.GetUsers().Count;
            var pendingBills = _billingService.GetBillingRecords().Count(b => b.Status.Equals("PENDING", System.StringComparison.OrdinalIgnoreCase));
            var totalEarnings = _billingService.GetBillingRecords().Where(b => b.Status.Equals("PAID", System.StringComparison.OrdinalIgnoreCase)).Sum(b => b.TotalAmount);

            return Ok(new
            {
                TotalPatients = patientsCount,
                ActiveAdmissions = activeAdmissions,
                TotalStaff = staffCount,
                PendingPayments = pendingBills,
                RevenueGenerated = totalEarnings
            });
        }

        [HttpGet("/api/search")]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new System.Collections.Generic.List<object>());
            }

            var results = new System.Collections.Generic.List<object>();

            if (User.IsInRole("admin"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={p.PatientId}"
                    });

                var staff = _userService.GetUsers()
                    .Where(u => u.FullName.Contains(query, System.StringComparison.OrdinalIgnoreCase) || u.Username.Contains(query, System.StringComparison.OrdinalIgnoreCase))
                    .Select(u => new
                    {
                        title = u.FullName,
                        subtitle = $"Staff Member ({u.Role})",
                        url = u.Role.Equals("doctor", System.StringComparison.OrdinalIgnoreCase) ? $"/admin/doctors" : $"/admin/dashboard"
                    });

                results.AddRange(patients);
                results.AddRange(staff);
            }
            else if (User.IsInRole("receptionist"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query) || p.ContactNumber.Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/receptionist/admission?search={p.PatientId}"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("doctor"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = p.Status == "ADMITTED" ? $"/doctor/ehr?patientId={p.PatientId}" : $"/doctor/dashboard"
                    });

                results.AddRange(patients);
            }
            else if (User.IsInRole("billing discharge"))
            {
                var patients = _patientService.GetPatients()
                    .Where(p => p.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) || p.PatientId.ToString().Contains(query))
                    .Select(p => new
                    {
                        title = p.Name,
                        subtitle = $"Patient (#{p.PatientId}) - Status: {p.Status}",
                        url = $"/billing/payments?patientId={p.PatientId}"
                    });

                results.AddRange(patients);
            }

            return Ok(results);
        }
    }
}
