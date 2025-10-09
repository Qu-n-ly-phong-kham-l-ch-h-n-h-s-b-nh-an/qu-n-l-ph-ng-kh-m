using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // bắt buộc đăng nhập
    public class SpecialtiesController : ControllerBase
    {
        private readonly SpecialtyService _specialtyService;

        public SpecialtiesController(SpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        // ✅ GET ALL
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _specialtyService.GetAll()
                .Select(s => new SpecialtyDTO
                {
                    SpecialtyId = s.SpecialtyId,
                    SpecialtyName = s.SpecialtyName
                });

            return Ok(list);
        }

        // ✅ GET BY ID
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var s = _specialtyService.GetById(id);
            if (s == null) return NotFound("Không tìm thấy chuyên khoa.");

            return Ok(new SpecialtyDTO
            {
                SpecialtyId = s.SpecialtyId,
                SpecialtyName = s.SpecialtyName
            });
        }

        // ✅ CREATE
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] SpecialtyDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var s = new Specialty { SpecialtyName = dto.SpecialtyName };
                _specialtyService.Create(s);
                return Ok(new { message = "✅ Thêm chuyên khoa thành công!", s.SpecialtyId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ✅ UPDATE
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] SpecialtyDTO dto)
        {
            var existing = _specialtyService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy chuyên khoa.");

            existing.SpecialtyName = dto.SpecialtyName;
            _specialtyService.Update(existing);

            return Ok(new { message = "✏️ Cập nhật chuyên khoa thành công!" });
        }

        // ✅ DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _specialtyService.Delete(id);
                return Ok(new { message = "🗑️ Xóa chuyên khoa thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}