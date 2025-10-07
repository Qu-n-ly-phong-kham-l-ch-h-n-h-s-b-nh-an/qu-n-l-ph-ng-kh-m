using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // yêu cầu đăng nhập
    public class SpecialtiesController : ControllerBase
    {
        private readonly SpecialtyService _specialtyService;

        public SpecialtiesController(SpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        // ======================== 1️⃣ ADMIN & LỄ TÂN XEM DANH SÁCH CHUYÊN KHOA ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public ActionResult<IEnumerable<SpecialtyResponseDTO>> GetAll()
        {
            var specialties = _specialtyService.GetAll()
                .Select(s => new SpecialtyResponseDTO
                {
                    SpecialtyId = s.SpecialtyId,
                    SpecialtyName = s.SpecialtyName
                });

            return Ok(specialties);
        }

        // ======================== 2️⃣ XEM CHI TIẾT CHUYÊN KHOA ========================
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public ActionResult<SpecialtyResponseDTO> GetById(int id)
        {
            var s = _specialtyService.GetById(id);
            if (s == null)
                return NotFound("Không tìm thấy chuyên khoa.");

            var dto = new SpecialtyResponseDTO
            {
                SpecialtyId = s.SpecialtyId,
                SpecialtyName = s.SpecialtyName
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ ADMIN THÊM CHUYÊN KHOA ========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] SpecialtyRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var specialty = new Specialty
            {
                SpecialtyName = dto.SpecialtyName
            };

            _specialtyService.Add(specialty);
            return Ok(new { message = "Thêm chuyên khoa thành công!", specialtyId = specialty.SpecialtyId });
        }

        // ======================== 4️⃣ ADMIN CẬP NHẬT ========================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] SpecialtyRequestDTO dto)
        {
            var existing = _specialtyService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy chuyên khoa.");

            existing.SpecialtyName = dto.SpecialtyName;
            _specialtyService.Update(existing);
            return Ok(new { message = "Cập nhật chuyên khoa thành công!" });
        }

        // ======================== 5️⃣ ADMIN XÓA ========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var specialty = _specialtyService.GetById(id);
            if (specialty == null)
                return NotFound("Không tìm thấy chuyên khoa.");

            _specialtyService.Delete(id);
            return Ok(new { message = "Xóa chuyên khoa thành công!" });
        }
    }
}
