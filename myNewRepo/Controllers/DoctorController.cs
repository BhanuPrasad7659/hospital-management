using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Models;
using CogMediHospitalManagementSystem.ViewModels;
using CogMediHospitalManagementSystem.DTOs;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CogMediHospitalManagementSystem.Controllers
{
    [Authorize(Roles = "admin,doctor")]
    public class DoctorController : BaseController
    {
        public DoctorController()
        {
        }

        private async Task<(int doctorId, string doctorName, string doctorUniqueId)> GetCurrentDoctorInfoAsync()
        {
            var doctorUsername = User.Identity?.Name ?? "";
            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();

            var doctorUser = users.FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase) && u.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase));
            int docId = doctorUser?.Id ?? (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0);
            string docName = doctorUser?.FullName ?? User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Dr. Ramesh";
            string uniqueId = $"D{docId:D3}";

            return (docId, docName, uniqueId);
        }

        [Route("doctor/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var (doctorId, doctorName, doctorUniqueId) = await GetCurrentDoctorInfoAsync();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            var treatments = await GetAsync<List<TreatmentPlan>>("api/doctor/treatments") ?? new List<TreatmentPlan>();

            var myPatients = patients.Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))).Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name,
                Age = p.Age,
                Gender = p.Gender,
                Address = p.Address,
                ContactNumber = p.ContactNumber,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                AssignedDoctorId = p.AssignedDoctorId,
                AssignedDoctorName = p.AssignedDoctorName,
                Ward = p.Ward,
                BedNumber = p.BedNumber
            }).ToList();

            var myTreatments = treatments.Where(t => t.DoctorId == doctorId || (t.DoctorName != null && t.DoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))).ToList();

            var viewModel = new DashboardViewModel
            {
                TotalPatients = myPatients.Count,
                ActiveCases = myPatients.Count,
                ActiveTreatments = myTreatments.Count,
                RecentAdmissions = myPatients.Take(5).ToList(),
                RecentActivities = new List<string>
                {
                    "Logged into Physician & Doctor Portal.",
                    "Reviewing active patient queue assigned by reception desk."
                },
                RoleName = "Medical Practitioner Desk",
                UserDisplayName = $"{doctorUniqueId} - {doctorName}"
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("doctor/ehr")]
        public async Task<IActionResult> Ehr(int? patientId)
        {
            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            List<EhrRecord> records = new List<EhrRecord>();
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                var selResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId.Value}");
                if (selResponse != null)
                {
                    selectedPatient = new Patient
                    {
                        PatientId = selResponse.PatientId,
                        Name = selResponse.Name,
                        Age = selResponse.Age,
                        Gender = selResponse.Gender,
                        Address = selResponse.Address,
                        ContactNumber = selResponse.ContactNumber,
                        Status = selResponse.Status
                    };
                }

                records = await GetAsync<List<EhrRecord>>($"api/doctor/ehr/patient/{patientId.Value}") ?? new List<EhrRecord>();
            }
            else
            {
                records = await GetAsync<List<EhrRecord>>("api/doctor/ehr") ?? new List<EhrRecord>();
            }

            ViewBag.PatientsList = patients.Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))).Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name
            }).ToList();

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.EhrRecords = records;
            return View(new EhrRecord { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/ehr")]
        public async Task<IActionResult> AddEhr(EhrRecord record)
        {
            if (record.PatientId <= 0 || string.IsNullOrEmpty(record.Diagnosis))
            {
                TempData["ErrorMessage"] = "Patient selection and Diagnosis are required.";
                return RedirectToAction("Ehr", new { patientId = record.PatientId });
            }

            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            record.DoctorId = doctorId;
            record.DoctorName = doctorName;

            var response = await SendAsync("api/doctor/ehr", HttpMethod.Post, record);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "EHR saved! Please order the necessary lab tests next.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to save EHR via API.";
            }

            return RedirectToAction("OrderLabTest", new { patientId = record.PatientId });
        }

        [HttpGet]
        [Route("doctor/order-lab-test")]
        public async Task<IActionResult> OrderLabTest(int? patientId)
        {
            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            var ehrRecords = await GetAsync<List<EhrRecord>>("api/doctor/ehr") ?? new List<EhrRecord>();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                var selResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId.Value}");
                if (selResponse != null)
                {
                    selectedPatient = new Patient { PatientId = selResponse.PatientId, Name = selResponse.Name };
                }
            }

            var patientsWithEhrIds = ehrRecords.Select(e => e.PatientId).Distinct().ToList();
            ViewBag.PatientsList = patients.Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase))) && patientsWithEhrIds.Contains(p.PatientId)).Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name
            }).ToList();

            ViewBag.SelectedPatient = selectedPatient;
            return View(new OrderTestLab { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/order-lab-test")]
        public async Task<IActionResult> OrderLabTest(int patientId, List<string> selectedTests)
        {
            if (selectedTests == null || !selectedTests.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one laboratory test.";
                return RedirectToAction("OrderLabTest", new { patientId = patientId });
            }

            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            var payload = new
            {
                PatientId = patientId,
                DoctorId = doctorId,
                DoctorName = doctorName,
                SelectedTests = selectedTests
            };

            var response = await SendAsync("api/doctor/lab-orders", HttpMethod.Post, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Ordered {selectedTests.Count} lab tests successfully! You can start treatment once results are uploaded.";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to order lab tests via API.";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        [Route("doctor/treatment")]
        public async Task<IActionResult> Treatment(int? patientId)
        {
            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            var stock = await GetAsync<Dictionary<string, int>>("api/pharmacist/stock") ?? new Dictionary<string, int>();
            List<TreatmentPlan> treatments = new List<TreatmentPlan>();
            List<OrderTestLab> labResults = new List<OrderTestLab>();
            Patient? selectedPatient = null;

            if (patientId.HasValue && patientId.Value > 0)
            {
                var selResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId.Value}");
                if (selResponse != null)
                {
                    selectedPatient = new Patient { PatientId = selResponse.PatientId, Name = selResponse.Name };
                }

                treatments = await GetAsync<List<TreatmentPlan>>($"api/doctor/treatments/patient/{patientId.Value}") ?? new List<TreatmentPlan>();
                labResults = await GetAsync<List<OrderTestLab>>($"api/lab/orders/patient/{patientId.Value}") ?? new List<OrderTestLab>();

                // Fetch latest EHR to find active diagnosis
                var ehrList = await GetAsync<List<EhrRecord>>($"api/doctor/ehr/patient/{patientId.Value}") ?? new List<EhrRecord>();
                var latestEhr = ehrList.OrderByDescending(e => e.VisitDate).FirstOrDefault();
                ViewBag.ActiveDiagnosis = latestEhr?.Diagnosis ?? "";
            }
            else
            {
                treatments = await GetAsync<List<TreatmentPlan>>("api/doctor/treatments") ?? new List<TreatmentPlan>();
                labResults = await GetAsync<List<OrderTestLab>>("api/lab/orders") ?? new List<OrderTestLab>();
            }

            ViewBag.PatientsList = patients.Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))).Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name
            }).ToList();

            ViewBag.SelectedPatient = selectedPatient;
            ViewBag.TreatmentHistory = treatments;
            ViewBag.PatientLabResults = labResults;

            var users = await GetAsync<List<UserDto>>("api/admin/users") ?? new List<UserDto>();
            var doctorUsername = User.Identity?.Name ?? "";
            var doctorUser = users.FirstOrDefault(u => u.Username.Equals(doctorUsername, StringComparison.OrdinalIgnoreCase));
            var specialty = doctorUser?.Specialty ?? "Physician";

            var specialtyMeds = new List<string>();
            if (specialty.Equals("Cardiology", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Atorvastatin 20mg", "Metoprolol 50mg", "Clopidogrel 75mg", "Aspirin 81mg", "Amlodipine 5mg", "Lisinopril 10mg", "Losartan 50mg", "Carvedilol 6.25mg", "Spironolactone 25mg", "Furosemide 40mg" };
            }
            else if (specialty.Equals("Pediatrics", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Amoxicillin 250mg Suspension", "Paracetamol 120mg Syrup", "Ibuprofen 100mg Suspension", "Cetirizine 5mg Syrup", "Zinc Drops 15ml", "Vitamin D3 Drops", "Oral Rehydration Salts (ORS)", "Salbutamol 2mg Syrup", "Cough Relief Pediatric", "Multivitamin Pediatric Syrup" };
            }
            else if (specialty.Equals("Orthopedics", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Diclofenac 50mg", "Ibuprofen 400mg", "Tramadol 50mg", "Calcium + Vitamin D3", "Methylsulfonylmethane (MSM)", "Glucosamine Chondroitin", "Aceclofenac 100mg", "Etoricoxib 90mg", "Pregabalin 75mg", "Paracetamol + Thiocolchicoside" };
            }
            else if (specialty.Equals("Neurology", StringComparison.OrdinalIgnoreCase))
            {
                specialtyMeds = new List<string> { "Gabapentin 300mg", "Levetiracetam 500mg", "Donepezil 5mg", "Sumatriptan 50mg", "Methylcobalamin 1500mcg", "Sodium Valproate 300mg", "Carbamazepine 200mg", "Amitriptyline 10mg", "Topiramate 50mg", "Clonazepam 0.5mg" };
            }
            else
            {
                specialtyMeds = new List<string> { "Amoxicillin 500mg", "Paracetamol 650mg", "Azithromycin 500mg", "Pantoprazole 40mg", "Cetirizine 10mg", "Ranitidine 150mg", "Omeprazole 20mg", "Dolo 650mg", "Cough Syrup Adults", "B-Complex with Zinc" };
            }

            ViewBag.AvailableMedicines = stock.Keys.Where(m => stock[m] > 0).OrderBy(m => m).ToList();
            ViewBag.SpecialtyMedicines = specialtyMeds;
            return View(new TreatmentPlan { PatientId = patientId ?? 0 });
        }

        [HttpPost]
        [Route("doctor/treatment")]
        public async Task<IActionResult> AddTreatment(TreatmentPlan plan, List<string> selectedMedicines)
        {
            if (plan.PatientId <= 0 || string.IsNullOrEmpty(plan.TreatmentDescription))
            {
                TempData["ErrorMessage"] = "Patient selection and Treatment Description are required.";
                return RedirectToAction("Treatment", new { patientId = plan.PatientId });
            }

            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            plan.DoctorId = doctorId;
            plan.DoctorName = doctorName;
            plan.Medication = selectedMedicines != null && selectedMedicines.Any()
                ? string.Join(", ", selectedMedicines)
                : "";

            var response = await SendAsync("api/doctor/treatments", HttpMethod.Post, plan);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Treatment plan prescribed successfully!";
            }
            else
            {
                var error = response != null ? await response.Content.ReadAsStringAsync() : "";
                TempData["ErrorMessage"] = !string.IsNullOrEmpty(error) ? error : "Failed to prescribe treatment plan via API.";
            }

            return RedirectToAction("Treatment", new { patientId = plan.PatientId });
        }

        [HttpGet]
        [Route("doctor/patient-history")]
        public async Task<IActionResult> PatientHistory(int? patientId)
        {
            var (doctorId, doctorName, _) = await GetCurrentDoctorInfoAsync();
            var patients = await GetAsync<List<PatientDto>>("api/receptionist/patients") ?? new List<PatientDto>();
            Patient? selectedPatient = null;
            List<TreatmentPlan> treatments = new List<TreatmentPlan>();
            List<OrderTestLab> labOrders = new List<OrderTestLab>();

            if (patientId.HasValue && patientId.Value > 0)
            {
                var selResponse = await GetAsync<PatientDto>($"api/receptionist/patients/{patientId.Value}");
                if (selResponse != null)
                {
                    selectedPatient = new Patient { PatientId = selResponse.PatientId, Name = selResponse.Name };
                }

                treatments = await GetAsync<List<TreatmentPlan>>($"api/doctor/treatments/patient/{patientId.Value}") ?? new List<TreatmentPlan>();
                labOrders = await GetAsync<List<OrderTestLab>>($"api/lab/orders/patient/{patientId.Value}") ?? new List<OrderTestLab>();
            }
            else
            {
                treatments = await GetAsync<List<TreatmentPlan>>("api/doctor/treatments") ?? new List<TreatmentPlan>();
                labOrders = await GetAsync<List<OrderTestLab>>("api/lab/orders") ?? new List<OrderTestLab>();
            }

            ViewBag.PatientsList = patients.Where(p => p.Status == "ADMITTED" && (p.AssignedDoctorId == doctorId || (p.AssignedDoctorName != null && p.AssignedDoctorName.Equals(doctorName, StringComparison.OrdinalIgnoreCase)))).Select(p => new Patient
            {
                PatientId = p.PatientId,
                Name = p.Name
            }).ToList();

            ViewBag.Patient = selectedPatient;
            ViewBag.Treatments = treatments;
            ViewBag.LabOrders = labOrders;
            return View();
        }

        [HttpGet]
        [Route("doctor/profile")]
        public async Task<IActionResult> Profile()
        {
            var (doctorId, _, _) = await GetCurrentDoctorInfoAsync();
            var response = await GetAsync<UserDto>($"api/admin/users/{doctorId}");
            User? doctor = null;

            if (response != null)
            {
                doctor = new User
                {
                    Id = response.Id,
                    Username = response.Username,
                    Role = response.Role,
                    FullName = response.FullName,
                    Specialty = response.Specialty,
                    Biography = response.Biography,
                    ContactNumber = response.ContactNumber,
                    Email = response.Email
                };
            }

            if (doctor == null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(doctor);
        }

        [HttpPost]
        [Route("doctor/profile/update")]
        public async Task<IActionResult> UpdateProfile(string specialty, string biography, string contactNumber, string email)
        {
            var (doctorId, _, _) = await GetCurrentDoctorInfoAsync();
            var payload = new UserUpdateDto
            {
                Specialty = string.IsNullOrWhiteSpace(specialty) ? null : specialty,
                Biography = string.IsNullOrWhiteSpace(biography) ? null : biography,
                ContactNumber = string.IsNullOrWhiteSpace(contactNumber) ? null : contactNumber,
                Email = string.IsNullOrWhiteSpace(email) ? null : email
            };

            var response = await SendAsync($"api/admin/users/{doctorId}", HttpMethod.Put, payload);
            if (response != null && response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Professional doctor profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update doctor profile via API.";
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("doctor/view-profile")]
        public async Task<IActionResult> ViewProfile(int doctorId)
        {
            var response = await GetAsync<UserDto>($"api/admin/users/{doctorId}");
            User? doctor = null;

            if (response != null && response.Role.Equals("doctor", StringComparison.OrdinalIgnoreCase))
            {
                doctor = new User
                {
                    Id = response.Id,
                    Username = response.Username,
                    Role = response.Role,
                    FullName = response.FullName,
                    Specialty = response.Specialty,
                    Biography = response.Biography,
                    ContactNumber = response.ContactNumber,
                    Email = response.Email
                };
            }

            if (doctor == null)
            {
                return Content("Doctor profile not found.");
            }
            return View(doctor);
        }
    }
}
