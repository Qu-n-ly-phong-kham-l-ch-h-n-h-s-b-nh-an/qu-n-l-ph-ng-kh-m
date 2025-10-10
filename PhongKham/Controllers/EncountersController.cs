using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu đăng nhập
    public class EncountersController : ControllerBase
    {
        private readonly EncounterService _service;

        public EncountersController(EncounterService service)
        {
            _service = service;
        }

        // ================== 1️⃣ LẤY DANH SÁCH ==================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _service.GetAll()
                .Select(e => new EncountersDTO
                {
                    EncounterId = e.EncounterId,
                    DoctorName = e.Doctor?.FullName,
                    PatientName = e.Appointment?.Patient?.FullName,
                    AppointmentDate = e.Appointment?.AppointmentDate ?? DateTime.MinValue,
                    Notes = e.Notes
                });
            return Ok(data);
        }

        // ================== 2️⃣ LẤY THEO ID ==================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var e = _service.GetById(id);
            if (e == null) return NotFound("Không tìm thấy lần khám.");

            var dto = new EncountersDTO
            {
                EncounterId = e.EncounterId,
                DoctorName = e.Doctor?.FullName,
                PatientName = e.Appointment?.Patient?.FullName,
                AppointmentDate = e.Appointment?.AppointmentDate ?? DateTime.MinValue,
                Notes = e.Notes
            };
            return Ok(dto);
        }

        // ================== 3️⃣ TẠO MỚI LẦN KHÁM ==================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] EncountersDTO dto)
        {
            try
            {
                var encounter = new Encounter
                {
                    AppointmentId = dto.AppointmentId,
                    DoctorId = dto.DoctorId,
                    Notes = dto.Notes
                };
                _service.Create(encounter);
                return Ok(new { message = "Thêm lần khám thành công!", encounter.EncounterId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================== 4️⃣ THÊM CHẨN ĐOÁN ==================
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("{encounterId}/diagnosis")]
        public IActionResult AddDiagnosis(int encounterId, [FromBody] DiagnosisDTO dto)
        {
            try
            {
                _service.AddDiagnosis(encounterId, dto.Description);
                return Ok(new { message = "Đã thêm chẩn đoán." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================== 5️⃣ THÊM ĐƠN THUỐC ==================
        [Authorize(Roles = "Admin,Doctor,Pharmacist")]
        [HttpPost("{encounterId}/prescription")]
        public IActionResult AddPrescription(int encounterId, [FromBody] PrescriptionDTO dto)
        {
            try
            {
                _service.AddPrescription(encounterId, dto.DrugId, dto.Quantity, dto.Usage);
                return Ok(new { message = "Đã kê thuốc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================== 6️⃣ HOÀN TẤT KHÁM (TẠO HÓA ĐƠN) ==================
        [Authorize(Roles = "Admin,Doctor")]
        [HttpPut("{id}/complete")]
        public IActionResult Complete(int id)
        {
            try
            {
                _service.CompleteEncounter(id);
                return Ok(new { message = "Hoàn tất lần khám và đã tạo hóa đơn." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ================== 🔍 TÌM KIẾM, LỌC, SẮP XẾP, PHÂN TRANG ==================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("search")]
        public IActionResult Search(
            string? keyword,
            string? doctorName,
            string? patientName,
            DateTime? fromDate,
            DateTime? toDate,
            string? sortBy = "date",
            bool ascending = false,
            int page = 1,
            int pageSize = 10)
        {
            var data = _service.Search(keyword, doctorName, patientName, fromDate, toDate, sortBy, ascending, page, pageSize)
                .Select(e => new EncountersDTO
                {
                    EncounterId = e.EncounterId,
                    DoctorName = e.Doctor?.FullName,
                    PatientName = e.Appointment?.Patient?.FullName,
                    AppointmentDate = e.Appointment?.AppointmentDate ?? DateTime.MinValue,
                    Notes = e.Notes
                });

            return Ok(data);
        }


    }
}