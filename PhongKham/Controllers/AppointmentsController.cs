using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;
using System.Linq;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;

        public AppointmentsController(AppointmentService appointmentService, DoctorService doctorService, PatientService patientService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _patientService = patientService;
        }

        // 🧾 Bệnh nhân đặt lịch
        [AllowAnonymous]
        [HttpPost("book")]
        public IActionResult Book([FromBody] AppointmentRequestDTO dto)
        {
            if (dto == null) return BadRequest("Thông tin lịch hẹn không hợp lệ.");

            var doctor = _doctorService.GetById(dto.DoctorId);
            var patient = _patientService.GetById(dto.PatientId);
            if (doctor == null || patient == null)
                return NotFound("Không tìm thấy bác sĩ hoặc bệnh nhân.");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                AppointmentDate = dto.AppointmentDate == default ? DateTime.Now : dto.AppointmentDate,
                Status = "Chờ xác nhận"
            };

            _appointmentService.Create(appointment);
            return Ok(new { message = "Đặt lịch thành công, vui lòng chờ xác nhận.", appointmentId = appointment.AppointmentId });
        }

        // 📋 Lễ tân / Admin xem toàn bộ lịch
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _appointmentService.GetAll().Select(a => new AppointmentResponseDTO
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor?.FullName,
                PatientName = a.Patient?.FullName,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status
            });
            return Ok(data);
        }

        // 🔍 Tìm kiếm / Lọc / Phân trang
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("filter")]
        public IActionResult Filter([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] string? sortBy, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = _appointmentService.Filter(keyword, status, sortBy, page, pageSize);
            return Ok(result);
        }

        // 🧑‍⚕️ Bác sĩ xem lịch của mình
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/{doctorId}")]
        public IActionResult GetByDoctor(int doctorId)
        {
            var list = _appointmentService.GetAll()
                .Where(a => a.DoctorId == doctorId)
                .Select(a => new AppointmentResponseDTO
                {
                    AppointmentId = a.AppointmentId,
                    DoctorName = a.Doctor?.FullName,
                    PatientName = a.Patient?.FullName,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                });
            return Ok(list);
        }

        // ✅ Duyệt lịch
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}/approve")]
        public IActionResult Approve(int id)
        {
            try
            {
                _appointmentService.Approve(id);
                return Ok(new { message = "Đã duyệt lịch hẹn và tạo lần khám tự động." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ❌ Hủy lịch
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            try
            {
                _appointmentService.Cancel(id);
                return Ok(new { message = "Đã hủy lịch hẹn." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 🧹 Xóa lịch (Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _appointmentService.Delete(id);
            return Ok(new { message = "Đã xóa lịch hẹn." });
        }
    }
}
