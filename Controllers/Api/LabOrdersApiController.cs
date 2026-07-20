using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/lab-orders")]
    public class LabOrdersApiController : ControllerBase
    {
        private readonly HospitalService _hospitalService;

        public LabOrdersApiController(HospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        // GET: api/lab-orders
        [HttpGet]
        public IActionResult GetLabOrders()
        {
            var orders = _hospitalService.GetLabOrders();
            return Ok(orders);
        }

        // GET: api/lab-orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetLabOrder(string id)
        {
            var order = _hospitalService.GetLabOrders().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order '{id}' not found.");
            return Ok(order);
        }

        // GET: api/lab-orders/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public IActionResult GetLabOrdersForPatient(string patientId)
        {
            var orders = _hospitalService.GetLabOrdersForPatient(patientId);
            return Ok(orders);
        }

        // POST: api/lab-orders
        [HttpPost]
        public IActionResult CreateLabOrder([FromBody] LabOrder order)
        {
            if (order == null || string.IsNullOrEmpty(order.PatientId) || string.IsNullOrEmpty(order.TestName))
            {
                return BadRequest("PatientId and TestName are required.");
            }

            var patient = _hospitalService.GetPatient(order.PatientId);
            if (patient == null) return NotFound($"Patient '{order.PatientId}' not found.");

            var created = _hospitalService.CreateLabOrder(order);
            return CreatedAtAction(nameof(GetLabOrder), new { id = created.LabOrderId }, created);
        }

        // PUT: api/lab-orders/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateLabOrder(string id, [FromBody] LabOrder updateDetails)
        {
            var order = _hospitalService.GetLabOrders().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order '{id}' not found.");

            _hospitalService.UpdateLabOrderStatus(id, updateDetails.Status, updateDetails.Result ?? "", updateDetails.TechnicianName ?? "");
            var updated = _hospitalService.GetLabOrders().FirstOrDefault(o => o.LabOrderId == id);
            return Ok(updated);
        }

        // DELETE: api/lab-orders/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteLabOrder(string id)
        {
            var order = _hospitalService.GetLabOrders().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order '{id}' not found.");

            _hospitalService.DeleteLabOrder(id);
            return NoContent();
        }
    }
}
