using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentsController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: api/appointments
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_appointmentService.GetAll());
        }

        // GET: api/appointments/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        // POST: api/appointments
        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            _appointmentService.Create(appointment);
            return Ok(appointment);
        }

        // PUT: api/appointments/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, Appointment appointment)
        {
            if (id != appointment.AppointmentId) return BadRequest();
            _appointmentService.Update(appointment);
            return Ok(appointment);
        }

        // DELETE: api/appointments/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _appointmentService.Delete(id);
            return NoContent();
        }
    }
}
