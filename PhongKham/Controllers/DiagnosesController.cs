using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosesController : ControllerBase
    {
        private readonly DiagnosisService _diagnosisService;

        public DiagnosesController(DiagnosisService diagnosisService)
        {
            _diagnosisService = diagnosisService;
        }

        // GET: api/diagnoses
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_diagnosisService.GetAll());
        }

        // GET: api/diagnoses/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var diag = _diagnosisService.GetById(id);
            if (diag == null) return NotFound();
            return Ok(diag);
        }

        // POST: api/diagnoses
        [HttpPost]
        public IActionResult Create(Diagnosis diagnosis)
        {
            _diagnosisService.Create(diagnosis);
            return Ok(diagnosis);
        }

        // PUT: api/diagnoses/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, Diagnosis diagnosis)
        {
            if (id != diagnosis.DiagnosisId) return BadRequest();
            _diagnosisService.Update(diagnosis);
            return Ok(diagnosis);
        }

        // DELETE: api/diagnoses/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _diagnosisService.Delete(id);
            return NoContent();
        }
    }
}
