using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // ✅ GET ALL
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _doctorService.GetAll()
                .Select(d => new DoctorDTO
                {
                    DoctorId = d.DoctorId,
                    FullName = d.FullName,
                    SpecialtyId = d.SpecialtyId,
                    SpecialtyName = d.Specialty?.SpecialtyName,
                    Phone = d.Phone,
                    Email = d.Email
                });

            return Ok(list);
        }

        // ✅ GET BY ID
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var d = _doctorService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy bác sĩ.");

            var dto = new DoctorDTO
            {
                DoctorId = d.DoctorId,
                FullName = d.FullName,
                SpecialtyId = d.SpecialtyId,
                SpecialtyName = d.Specialty?.SpecialtyName,
                Phone = d.Phone,
                Email = d.Email
            };

            return Ok(dto);
        }

        // ✅ CREATE
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] DoctorDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var doctor = new Doctor
                {
                    FullName = dto.FullName,
                    SpecialtyId = dto.SpecialtyId,
                    Phone = dto.Phone,
                    Email = dto.Email
                };

                _doctorService.Create(doctor);
                return Ok(new { message = "✅ Thêm bác sĩ thành công!", doctorId = doctor.DoctorId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ UPDATE
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DoctorDTO dto)
        {
            var existing = _doctorService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy bác sĩ.");

            existing.FullName = dto.FullName;
            existing.SpecialtyId = dto.SpecialtyId;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;

            _doctorService.Update(existing);
            return Ok(new { message = "✏️ Cập nhật thông tin bác sĩ thành công!" });
        }

        // ✅ DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _doctorService.Delete(id);
                return Ok(new { message = "🗑️ Xóa bác sĩ thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}