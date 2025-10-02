using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncountersController : ControllerBase
    {
        private readonly EncounterService _encounterService;

        public EncountersController(EncounterService encounterService)
        {
            _encounterService = encounterService;
        }

        // GET: api/encounters
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_encounterService.GetAll());
        }

        // GET: api/encounters/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var encounter = _encounterService.GetById(id);
            if (encounter == null) return NotFound();
            return Ok(encounter);
        }

        // POST: api/encounters
        [HttpPost]
        public IActionResult Create(Encounter encounter)
        {
            _encounterService.Create(encounter);
            return Ok(encounter);
        }

        // PUT: api/encounters/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, Encounter encounter)
        {
            if (id != encounter.EncounterId) return BadRequest();
            _encounterService.Update(encounter);
            return Ok(encounter);
        }

        // DELETE: api/encounters/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _encounterService.Delete(id);
            return NoContent();
        }
    }
}
