// File: Controllers/AppointmentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.DAL; // Thêm DAL
using QuanLyPhongKhamApi.Models;
using System.Security.Claims;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentBLL _bus;
        private readonly PatientDAL _patientDal;
        private readonly DoctorDAL _doctorDal;

        // ✅ HOÀN THIỆN: Inject các DAL cần thiết để kiểm tra quyền
        public AppointmentsController(AppointmentBLL bus, PatientDAL patientDal, DoctorDAL doctorDal)
        {
            _bus = bus;
            _patientDal = patientDal;
            _doctorDal = doctorDal;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public IActionResult GetAll([FromQuery] int? doctorId, [FromQuery] int? patientId, [FromQuery] DateTime? date, [FromQuery] string? status)
        {
            // ✅ HOÀN THIỆN: Phân quyền chi tiết cho bác sĩ
            if (User.IsInRole("Doctor"))
            {
                var accountId = int.Parse(User.FindFirstValue("AccountID")!);
                var doctorProfile = _doctorDal.GetByAccountId(accountId);

                // Bác sĩ chỉ được xem lịch của chính mình, không được xem của người khác
                if (doctorProfile != null)
                {
                    doctorId = doctorProfile.DoctorID;
                }
                else
                {
                    // Nếu tài khoản bác sĩ không liên kết với hồ sơ nào, trả về danh sách rỗng
                    return Ok(new List<AppointmentDTO>());
                }
            }

            var list = _bus.GetAll(doctorId, patientId, date, status);
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")]
        public IActionResult GetById(int id)
        {
            var appointment = _bus.GetById(id);
            if (appointment == null) return NotFound();

            // ✅ HOÀN THIỆN: Phân quyền chi tiết cho bệnh nhân
            if (User.IsInRole("Patient"))
            {
                var accountId = int.Parse(User.FindFirstValue("AccountID")!);
                var patientProfile = _patientDal.GetByAccountId(accountId);

                if (patientProfile == null || patientProfile.PatientID != appointment.PatientID)
                {
                    return Forbid(); // Trả về lỗi 403 Forbidden nếu không phải lịch hẹn của mình
                }
            }

            return Ok(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create([FromBody] AppointmentCreateRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var createdByAccountId = int.Parse(User.FindFirstValue("AccountID")!);
            var newId = _bus.Create(req, createdByAccountId);
            var newAppointment = _bus.GetById(newId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newAppointment);
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Receptionist,Patient")] // Bệnh nhân có thể tự hủy lịch
        public IActionResult Cancel(int id)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            var ok = _bus.Cancel(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy lịch hẹn để hủy." });
            return Ok(new { message = "Hủy lịch hẹn thành công." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var ok = _bus.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Update(int id, [FromBody] AppointmentUpdateRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound(new { message = "Không tìm thấy lịch hẹn để cập nhật." });
            return Ok(new { message = "Cập nhật lịch hẹn thành công." });
        }
    }
}