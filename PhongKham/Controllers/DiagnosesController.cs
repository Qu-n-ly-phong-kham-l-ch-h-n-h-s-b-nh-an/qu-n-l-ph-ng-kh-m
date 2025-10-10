using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ bắt buộc đăng nhập
    public class DiagnosesController : ControllerBase
    {
        private readonly DiagnosisService _diagnosisService;

        public DiagnosesController(DiagnosisService diagnosisService)
        {
            _diagnosisService = diagnosisService;
        }

        // ✅ 1️⃣ GET ALL (Tìm kiếm / Lọc / Sắp xếp / Phân trang)
        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public ActionResult<IEnumerable<DiagnosisDTO>> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] string? sortBy,
            [FromQuery] bool descending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var list = _diagnosisService.GetAll(keyword, sortBy, descending, page, pageSize)
                .Select(d => new DiagnosisDTO
                {
                    DiagnosisId = d.DiagnosisId,
                    EncounterId = d.EncounterId,
                    Description = d.Description,
                    ResultFile = d.ResultFile,
                    HasResultFile = d.ResultFile != null
                });

            return Ok(list);
        }

        // ✅ 2️⃣ GET BY ID
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("{id}")]
        public ActionResult<DiagnosisDTO> GetById(int id)
        {
            var d = _diagnosisService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy chẩn đoán.");

            var dto = new DiagnosisDTO
            {
                DiagnosisId = d.DiagnosisId,
                EncounterId = d.EncounterId,
                Description = d.Description,
                ResultFile = d.ResultFile,
                HasResultFile = d.ResultFile != null
            };

            return Ok(dto);
        }

        // ✅ 3️⃣ CREATE
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] DiagnosisDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var diagnosis = new Diagnosis
            {
                EncounterId = dto.EncounterId,
                Description = dto.Description,
                ResultFile = dto.ResultFile
            };

            _diagnosisService.Create(diagnosis);
            return Ok(new { message = "Thêm chẩn đoán thành công!", diagnosisId = diagnosis.DiagnosisId });
        }

        // ✅ 4️⃣ UPDATE
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DiagnosisDTO dto)
        {
            var existing = _diagnosisService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy chẩn đoán.");

            existing.Description = dto.Description;
            existing.ResultFile = dto.ResultFile;

            _diagnosisService.Update(existing);
            return Ok(new { message = "Cập nhật chẩn đoán thành công!" });
        }

        // ✅ 5️⃣ DELETE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var d = _diagnosisService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy chẩn đoán.");

            _diagnosisService.Delete(id);
            return Ok(new { message = "Xóa chẩn đoán thành công!" });
        }
    }
}
