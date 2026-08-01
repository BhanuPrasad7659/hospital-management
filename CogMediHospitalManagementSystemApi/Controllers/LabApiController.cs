using Microsoft.AspNetCore.Mvc;
using CogMediHospitalManagementSystem.Services.Interfaces;
using CogMediHospitalManagementSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace CogMediHospitalManagementSystemApi.Controllers
{
    [ApiController]
    [Route("api/lab")]
    [Authorize(Roles = "admin,laboratory,doctor")]
    public class LabApiController : ControllerBase
    {
        private readonly IOrderTestLabService _orderTestLabService;
        private readonly IPatientService _patientService;

        public LabApiController(IOrderTestLabService orderTestLabService, IPatientService patientService)
        {
            _orderTestLabService = orderTestLabService;
            _patientService = patientService;
        }

        [HttpGet("orders")]
        public IActionResult GetLabOrders()
        {
            var orders = _orderTestLabService.GetOrderTestLabs();
            return Ok(orders);
        }

        [HttpGet("orders/{id:int}")]
        public IActionResult GetLabOrder(int id)
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order #{id} not found.");
            return Ok(order);
        }

        [HttpGet("orders/patient/{patientId:int}")]
        public IActionResult GetLabOrdersForPatient(int patientId)
        {
            var orders = _orderTestLabService.GetOrderTestLabsForPatient(patientId);
            return Ok(orders);
        }

        [HttpPost("orders")]
        public IActionResult CreateLabOrder([FromBody] OrderTestLab order)
        {
            if (order == null || order.PatientId <= 0 || string.IsNullOrEmpty(order.TestName))
            {
                return BadRequest("PatientId and TestName are required.");
            }

            var patient = _patientService.GetPatient(order.PatientId);
            if (patient == null) return NotFound($"Patient #{order.PatientId} not found.");

            var created = _orderTestLabService.CreateOrderTestLab(order);
            return CreatedAtAction(nameof(GetLabOrder), new { id = created.LabOrderId }, created);
        }

        [HttpPut("orders/{id:int}")]
        public IActionResult UpdateLabOrder(int id, [FromBody] OrderTestLab updateDetails)
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order #{id} not found.");

            _orderTestLabService.UpdateOrderTestLabStatus(id, updateDetails.Status, updateDetails.Result ?? "", updateDetails.TechnicianName ?? "");
            var updated = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id);
            return Ok(updated);
        }

        [HttpDelete("orders/{id:int}")]
        public IActionResult DeleteLabOrder(int id)
        {
            var order = _orderTestLabService.GetOrderTestLabs().FirstOrDefault(o => o.LabOrderId == id);
            if (order == null) return NotFound($"Lab order #{id} not found.");

            _orderTestLabService.DeleteOrderTestLab(id);
            return NoContent();
        }
    }
}
