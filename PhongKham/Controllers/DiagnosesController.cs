using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ Bắt buộc phải đăng nhập
    public class DiagnosesController : ControllerBase
    {
        private readonly DiagnosisService _diagnosisService;

        public DiagnosesController(DiagnosisService diagnosisService)
        {
            _diagnosisService = diagnosisService;
        }

        // ======================== 1️⃣ ADMIN & BÁC SĨ XEM TẤT CẢ ========================
        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public ActionResult<IEnumerable<DiagnosisResponseDTO>> GetAll()
        {
            var list = _diagnosisService.GetAll()
                .Select(d => new DiagnosisResponseDTO
                {
                    DiagnosisId = d.DiagnosisId,
                    EncounterId = d.EncounterId,
                    Description = d.Description,
                    HasResultFile = d.ResultFile != null
                });
            return Ok(list);
        }

        // ======================== 2️⃣ XEM CHI TIẾT CHẨN ĐOÁN ========================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("{id}")]
        public ActionResult<DiagnosisResponseDTO> GetById(int id)
        {
            var d = _diagnosisService.GetById(id);
            if (d == null)
                return NotFound("Không tìm thấy chẩn đoán.");

            var dto = new DiagnosisResponseDTO
            {
                DiagnosisId = d.DiagnosisId,
                EncounterId = d.EncounterId,
                Description = d.Description,
                HasResultFile = d.ResultFile != null
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ BÁC SĨ THÊM CHẨN ĐOÁN ========================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        public IActionResult Create([FromBody] DiagnosisRequestDTO dto)
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

        // ======================== 4️⃣ BÁC SĨ CẬP NHẬT ========================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] DiagnosisRequestDTO dto)
        {
            var existing = _diagnosisService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy chẩn đoán.");

            _diagnosisService.Delete(id);
            return Ok(new { message = "Xóa chẩn đoán thành công!" });
        }
    }
}