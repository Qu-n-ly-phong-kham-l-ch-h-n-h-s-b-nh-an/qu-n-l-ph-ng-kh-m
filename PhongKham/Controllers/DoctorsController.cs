using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: api/Doctors
        [HttpGet]
        public ActionResult<IEnumerable<Doctor>> GetDoctors()
        {
            return Ok(_doctorService.GetAll());
        }

        // GET: api/Doctors/5
        [HttpGet("{id}")]
        public ActionResult<Doctor> GetDoctor(int id)
        {
            var doctor = _doctorService.GetById(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return Ok(doctor);
        }

        // POST: api/Doctors
        [HttpPost]
        public ActionResult<Doctor> CreateDoctor(Doctor doctor)
        {
            _doctorService.Create(doctor);
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
        }

        // PUT: api/Doctors/5
        [HttpPut("{id}")]
        public IActionResult UpdateDoctor(int id, Doctor doctor)
        {
            if (id != doctor.DoctorId)
            {
                return BadRequest();
            }

            var existing = _doctorService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _doctorService.Update(doctor);
            return NoContent();
        }

        // DELETE: api/Doctors/5
        [HttpDelete("{id}")]
        public IActionResult DeleteDoctor(int id)
        {
            var existing = _doctorService.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            _doctorService.Delete(id);
            return NoContent();
        }
    }
}
