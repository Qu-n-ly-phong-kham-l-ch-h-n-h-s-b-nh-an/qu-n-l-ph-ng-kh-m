using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ yêu cầu đăng nhập
    public class EncountersController : ControllerBase
    {
        private readonly EncounterService _encounterService;

        public EncountersController(EncounterService encounterService)
        {
            _encounterService = encounterService;
        }

        // ======================== 1️⃣ ADMIN, BÁC SĨ, LỄ TÂN XEM DANH SÁCH ========================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet]
        public ActionResult<IEnumerable<EncounterResponseDTO>> GetAll()
        {
            var list = _encounterService.GetAll()
                .Select(e => new EncounterResponseDTO
                {
                    EncounterId = e.EncounterId,
                    DoctorName = e.Doctor?.FullName ?? "(Không có bác sĩ)",
                    PatientName = e.Appointment?.Patient?.FullName ?? "(Không có bệnh nhân)",
                    AppointmentDate = e.Appointment?.AppointmentDate ?? DateTime.MinValue,
                    Notes = e.Notes
                });

            return Ok(list);
        }

        // ======================== 2️⃣ XEM CHI TIẾT ========================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("{id}")]
        public ActionResult<EncounterResponseDTO> GetById(int id)
        {
            var e = _encounterService.GetById(id);
            if (e == null) return NotFound("Không tìm thấy lần khám.");

            var dto = new EncounterResponseDTO
            {
                EncounterId = e.EncounterId,
                DoctorName = e.Doctor?.FullName,
                PatientName = e.Appointment?.Patient?.FullName,
                AppointmentDate = e.Appointment?.AppointmentDate ?? DateTime.MinValue,
                Notes = e.Notes
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ TẠO MỚI (CHỈ BÁC SĨ HOẶC LỄ TÂN) ========================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] EncounterRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var encounter = new Encounter
            {
                AppointmentId = dto.AppointmentId,
                DoctorId = dto.DoctorId,
                Notes = dto.Notes
            };

            _encounterService.Create(encounter);
            return Ok(new { message = "Thêm lần khám thành công!", encounterId = encounter.EncounterId });
        }

        // ======================== 4️⃣ CẬP NHẬT (BÁC SĨ HOẶC ADMIN) ========================
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EncounterRequestDTO dto)
        {
            var existing = _encounterService.GetById(id);
            if (existing == null) return NotFound("Không tìm thấy lần khám.");

            existing.AppointmentId = dto.AppointmentId;
            existing.DoctorId = dto.DoctorId;
            existing.Notes = dto.Notes;

            _encounterService.Update(existing);
            return Ok(new { message = "Cập nhật lần khám thành công!" });
        }

        // ======================== 5️⃣ XÓA (CHỈ ADMIN) ========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var encounter = _encounterService.GetById(id);
            if (encounter == null)
                return NotFound("Không tìm thấy lần khám.");

            _encounterService.Delete(id);
            return Ok(new { message = "Xóa lần khám thành công!" });
        }
    }
}