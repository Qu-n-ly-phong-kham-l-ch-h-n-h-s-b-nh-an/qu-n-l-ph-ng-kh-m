using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtiesController : ControllerBase
    {
        private readonly SpecialtyService _service = new SpecialtyService();

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var specialty = _service.GetById(id);
            if (specialty == null) return NotFound();
            return Ok(specialty);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Specialty specialty)
        {
            _service.Add(specialty);
            return Ok(specialty);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Specialty specialty)
        {
            if (id != specialty.SpecialtyId) return BadRequest();
            _service.Update(specialty);
            return Ok(specialty);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok();
        }
    }
}
