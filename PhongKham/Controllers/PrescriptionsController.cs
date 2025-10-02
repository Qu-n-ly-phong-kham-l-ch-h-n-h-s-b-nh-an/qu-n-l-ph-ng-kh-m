using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly PrescriptionService _prescriptionService;

        public PrescriptionsController(PrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // GET: api/prescriptions
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_prescriptionService.GetAll());
        }

        // GET: api/prescriptions/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var prescription = _prescriptionService.GetById(id);
            if (prescription == null) return NotFound();
            return Ok(prescription);
        }

        // POST: api/prescriptions
        [HttpPost]
        public IActionResult Create([FromBody] Prescription prescription)
        {
            _prescriptionService.Create(prescription);
            return Ok(prescription);
        }

        // PUT: api/prescriptions/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Prescription prescription)
        {
            if (id != prescription.PrescriptionId) return BadRequest();
            _prescriptionService.Update(prescription);
            return Ok(prescription);
        }

        // DELETE: api/prescriptions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _prescriptionService.Delete(id);
            return NoContent();
        }
    }
}
