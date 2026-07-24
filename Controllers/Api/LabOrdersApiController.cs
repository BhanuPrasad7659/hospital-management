using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;

namespace CogMediHospitalManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/lab-orders")]
    public class LabOrdersApiController : ControllerBase
    {
                private readonly IOrderTestLabService _orderTestLabService;
        private readonly IPatientService _patientService;

        public LabOrdersApiController(IOrderTestLabService orderTestLabService, IPatientService patientService)
        {
            _orderTestLabService = orderTestLabService;
            _patientService = patientService;
        }

        // GET: api/lab-orders
        [HttpGet]
        public IActionResult GetLabOrders()
        {
            var orders = _orderTestLabService.GetOrderTestLabs(); // Updated
            return Ok(orders);
        }

        // GET: api/lab-orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetLabOrder(int id)
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id); // Updated
            if (order == null) return NotFound($"Lab order #{id} not found.");
            return Ok(order);
        }

        // GET: api/lab-orders/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public IActionResult GetLabOrdersForPatient(int patientId)
        {
            var orders = _orderTestLabService.GetOrderTestLabsForPatient(patientId); // Updated
            return Ok(orders);
        }

        // POST: api/lab-orders
        [HttpPost]
        public IActionResult CreateLabOrder([FromBody] OrderTestLab order) // Updated
        {
            if (order == null || order.PatientId <= 0 || string.IsNullOrEmpty(order.TestName))
            {
                return BadRequest("PatientId and TestName are required.");
            }

            var patient = _patientService.GetPatient(order.PatientId);
            if (patient == null) return NotFound($"Patient #{order.PatientId} not found.");

            var created = _orderTestLabService.CreateOrderTestLab(order); // Updated
            return CreatedAtAction(nameof(GetLabOrder), new { id = created.LabOrderId }, created);
        }

        // PUT: api/lab-orders/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateLabOrder(int id, [FromBody] OrderTestLab updateDetails) // Updated
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id); // Updated
            if (order == null) return NotFound($"Lab order #{id} not found.");

            _orderTestLabService.UpdateOrderTestLabStatus(id, updateDetails.Status, updateDetails.Result ?? "", updateDetails.TechnicianName ?? ""); // Updated
            var updated = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id); // Updated
            return Ok(updated);
        }

        // DELETE: api/lab-orders/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteLabOrder(int id)
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id); // Updated
            if (order == null) return NotFound($"Lab order #{id} not found.");

            _orderTestLabService.DeleteOrderTestLab(id); // Updated
            return NoContent();
        }
    }
}