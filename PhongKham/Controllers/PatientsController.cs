using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: api/patients
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_patientService.GetAll());
        }

        // GET: api/patients/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var patient = _patientService.GetById(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        // POST: api/patients
        [HttpPost]
        public IActionResult Create([FromBody] Patient patient)
        {
            _patientService.Create(patient);
            return Ok(patient);
        }

        // PUT: api/patients/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Patient patient)
        {
            if (id != patient.PatientId) return BadRequest();
            _patientService.Update(patient);
            return Ok(patient);
        }

        // DELETE: api/patients/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _patientService.Delete(id);
            return NoContent();
        }
    }
}
