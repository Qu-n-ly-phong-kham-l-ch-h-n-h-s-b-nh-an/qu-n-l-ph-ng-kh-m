using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugsController : ControllerBase
    {
        private readonly DrugService _drugService;

        public DrugsController(DrugService drugService)
        {
            _drugService = drugService;
        }

        // GET: api/drugs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_drugService.GetAll());
        }

        // GET: api/drugs/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var drug = _drugService.GetById(id);
            if (drug == null) return NotFound();
            return Ok(drug);
        }

        // POST: api/drugs
        [HttpPost]
        public IActionResult Create(Drug drug)
        {
            _drugService.Create(drug);
            return Ok(drug);
        }

        // PUT: api/drugs/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, Drug drug)
        {
            if (id != drug.DrugId) return BadRequest();
            _drugService.Update(drug);
            return Ok(drug);
        }

        // DELETE: api/drugs/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _drugService.Delete(id);
            return NoContent();
        }
    }
}
