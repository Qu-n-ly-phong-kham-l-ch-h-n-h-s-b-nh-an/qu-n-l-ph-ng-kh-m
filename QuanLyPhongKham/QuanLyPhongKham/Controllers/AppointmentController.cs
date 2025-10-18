// File: Controllers/AppointmentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.DAL; // Cần thiết cho PatientDAL, DoctorDAL
using QuanLyPhongKhamApi.Models;
using System.Security.Claims;
using System; // Cần thiết cho DateTime
using System.Collections.Generic; // Cần thiết cho List<>

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize] // Yêu cầu xác thực cho tất cả
    [ApiController]
    [Route("api/[controller]")] // Route chuẩn /api/Appointments
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentBLL _bus;
        private readonly PatientDAL _patientDal; // Để lấy patientId cho Patient
        private readonly DoctorDAL _doctorDal;   // Để lấy doctorId cho Doctor

        public AppointmentsController(AppointmentBLL bus, PatientDAL patientDal, DoctorDAL doctorDal)
        {
            _bus = bus;
            _patientDal = patientDal;
            _doctorDal = doctorDal;
        }

        // GET /api/Appointments
        // === SỬA ĐỔI: Thêm vai trò "Patient" vào đây ===
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")] // Cho phép cả Patient
        public IActionResult GetAll([FromQuery] int? doctorId, [FromQuery] int? patientId, [FromQuery] DateTime? date, [FromQuery] string? status)
        {
            int? filterDoctorId = doctorId;
            int? filterPatientId = patientId;

            // Xử lý quyền truy cập chi tiết theo vai trò
            if (User.IsInRole("Doctor"))
            {
                var accountId = int.Parse(User.FindFirstValue("AccountID")!);
                var doctorProfile = _doctorDal.GetByAccountId(accountId);
                // Nếu bác sĩ không có hồ sơ hoặc cố gắng xem lịch của bác sĩ khác -> Lỗi
                if (doctorProfile == null || (filterDoctorId.HasValue && filterDoctorId != doctorProfile.DoctorID))
                {
                    return Forbid(); // Không có quyền xem lịch của bác sĩ khác
                }
                // Bác sĩ chỉ được xem lịch của chính mình
                filterDoctorId = doctorProfile.DoctorID;
                // Bác sĩ có thể lọc theo bệnh nhân cụ thể nếu muốn
                filterPatientId = patientId;
            }
            else if (User.IsInRole("Patient"))
            {
                var accountId = int.Parse(User.FindFirstValue("AccountID")!);
                var patientProfile = _patientDal.GetByAccountId(accountId);
                // Nếu bệnh nhân không có hồ sơ hoặc cố gắng xem lịch của người khác -> Lỗi
                if (patientProfile == null || (filterPatientId.HasValue && filterPatientId != patientProfile.PatientID))
                {
                    // Có thể trả về Forbid() hoặc danh sách rỗng tùy logic mong muốn
                    // return Forbid();
                    return Ok(new List<AppointmentDTO>()); // Trả về danh sách rỗng nếu không tìm thấy hồ sơ BN liên kết
                }
                // Bệnh nhân chỉ được xem lịch của chính mình
                filterPatientId = patientProfile.PatientID;
                // Bệnh nhân không được lọc theo bác sĩ (vì chỉ xem lịch của mình)
                filterDoctorId = null;
            }
            // Admin và Receptionist có thể xem/lọc tất cả (không cần điều kiện if riêng)

            // Gọi BLL với các tham số đã được lọc theo quyền
            var list = _bus.GetAll(filterDoctorId, filterPatientId, date, status);
            return Ok(list);
        }

        // GET /api/Appointments/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")] // Cho phép Patient xem chi tiết lịch của mình
        public IActionResult GetById(int id)
        {
            var appointment = _bus.GetById(id);
            if (appointment == null) return NotFound();

            // Kiểm tra quyền sở hữu cho Doctor và Patient
            var accountId = int.Parse(User.FindFirstValue("AccountID")!);

            if (User.IsInRole("Doctor"))
            {
                var doctorProfile = _doctorDal.GetByAccountId(accountId);
                if (doctorProfile == null || appointment.DoctorID != doctorProfile.DoctorID)
                {
                    return Forbid(); // Bác sĩ chỉ xem được lịch của mình
                }
            }
            else if (User.IsInRole("Patient"))
            {
                var patientProfile = _patientDal.GetByAccountId(accountId);
                if (patientProfile == null || appointment.PatientID != patientProfile.PatientID)
                {
                    return Forbid(); // Bệnh nhân chỉ xem được lịch của mình
                }
            }
            // Admin và Receptionist được xem tất cả

            return Ok(appointment);
        }

        // POST /api/Appointments (Tạo lịch hẹn)
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")] // Chỉ Admin, Lễ tân được tạo
        public IActionResult Create([FromBody] AppointmentCreateRequest req)
        {
            // Middleware xử lý lỗi ArgumentException, ApplicationException
            var createdByAccountId = int.Parse(User.FindFirstValue("AccountID")!);
            var newId = _bus.Create(req, createdByAccountId);
            var newAppointment = _bus.GetById(newId); // Lấy lại thông tin đầy đủ để trả về
            if (newAppointment == null)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin lịch hẹn vừa tạo." });
            }
            return CreatedAtAction(nameof(GetById), new { id = newId }, newAppointment);
        }

        // PATCH /api/Appointments/{id}/cancel (Hủy lịch hẹn)
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,Receptionist,Patient")] // Bệnh nhân có thể tự hủy lịch
        public IActionResult Cancel(int id)
        {
            // Middleware xử lý lỗi ApplicationException nếu trạng thái không hợp lệ
            var accountId = int.Parse(User.FindFirstValue("AccountID")!);
            bool canCancel = false;

            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                canCancel = true; // Admin/Lễ tân có thể hủy bất kỳ lịch nào (hợp lệ)
            }
            else if (User.IsInRole("Patient"))
            {
                // Kiểm tra xem lịch hẹn này có phải của bệnh nhân đang đăng nhập không
                var appointment = _bus.GetById(id);
                if (appointment != null)
                {
                    var patientProfile = _patientDal.GetByAccountId(accountId);
                    if (patientProfile != null && appointment.PatientID == patientProfile.PatientID)
                    {
                        canCancel = true;
                    }
                }
            }

            if (!canCancel)
            {
                return Forbid(); // Không có quyền hủy lịch này
            }

            var ok = _bus.Cancel(id); // BLL sẽ kiểm tra trạng thái trước khi hủy
            if (!ok) return NotFound(new { message = "Không tìm thấy lịch hẹn hoặc không thể hủy ở trạng thái hiện tại." });
            return Ok(new { message = "Hủy lịch hẹn thành công." });
        }

        // DELETE /api/Appointments/{id} (Xóa mềm - Chỉ Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var ok = _bus.Delete(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy lịch hẹn để xóa." });
            return NoContent(); // 204 No Content
        }

        // PUT /api/Appointments/{id} (Cập nhật lịch hẹn - Chỉ Admin/Lễ tân)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Update(int id, [FromBody] AppointmentUpdateRequest req)
        {
            // Middleware xử lý lỗi ArgumentException, ApplicationException
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound(new { message = "Không tìm thấy lịch hẹn hoặc không thể cập nhật." });
            return Ok(new { message = "Cập nhật lịch hẹn thành công." });
        }
    }
}