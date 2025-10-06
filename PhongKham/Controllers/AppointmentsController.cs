using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

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

        // ======================== 1️⃣ BỆNH NHÂN ĐẶT LỊCH ========================
        [AllowAnonymous]
        [HttpPost("book")]
        public IActionResult BookAppointment([FromBody] AppointmentRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Thông tin lịch hẹn không hợp lệ.");

            var doctor = _doctorService.GetById(dto.DoctorId);
            var patient = _patientService.GetById(dto.PatientId);

            if (doctor == null)
                return NotFound("Không tìm thấy bác sĩ.");
            if (patient == null)
                return NotFound("Không tìm thấy bệnh nhân.");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                AppointmentDate = dto.AppointmentDate == default ? DateTime.Now : dto.AppointmentDate,
                Status = "Chờ xác nhận"
            };

            _appointmentService.Create(appointment);

            return Ok(new
            {
                message = "Đặt lịch thành công! Vui lòng chờ xác nhận.",
                appointmentId = appointment.AppointmentId
            });
        }

        // ======================== 2️⃣ ADMIN & LỄ TÂN XEM TẤT CẢ ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _appointmentService.GetAll();

            var result = list.Select(a => new AppointmentResponseDTO
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor?.FullName,
                PatientName = a.Patient?.FullName,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status
            });

            return Ok(result);
        }

        // ======================== 3️⃣ BÁC SĨ XEM LỊCH CỦA MÌNH ========================
        [Authorize(Roles = "Doctor")]
        [HttpGet("by-doctor/{doctorId}")]
        public IActionResult GetByDoctor(int doctorId)
        {
            var list = _appointmentService
                .GetAll()
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

        // ======================== 4️⃣ ADMIN & LỄ TÂN XÁC NHẬN LỊCH ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}/confirm")]
        public IActionResult Confirm(int id)
        {
            var appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound("Không tìm thấy lịch hẹn.");

            appointment.Status = "Đã xác nhận";
            _appointmentService.Update(appointment);

            return Ok(new { message = "Đã xác nhận lịch hẹn.", appointment });
        }

        // ======================== 5️⃣ ADMIN & LỄ TÂN HỦY LỊCH ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            var appointment = _appointmentService.GetById(id);
            if (appointment == null) return NotFound("Không tìm thấy lịch hẹn.");

            appointment.Status = "Đã hủy";
            _appointmentService.Update(appointment);

            return Ok(new { message = "Đã hủy lịch hẹn.", appointment });
        }

        // ======================== 6️⃣ ADMIN XÓA LỊCH ========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var appointment = _appointmentService.GetById(id);
            if (appointment == null)
                return NotFound("Không tìm thấy lịch hẹn để xóa.");

            _appointmentService.Delete(id);
            return Ok(new { message = "Đã xóa lịch hẹn." });
        }
    }
}
