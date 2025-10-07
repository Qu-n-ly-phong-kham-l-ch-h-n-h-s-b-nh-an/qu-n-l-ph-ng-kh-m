using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using PhongKham.API.Models.DTOs;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu phải đăng nhập
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // ======================== 1️⃣ ADMIN & LỄ TÂN XEM DANH SÁCH BÁC SĨ ========================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public ActionResult<IEnumerable<DoctorResponseDTO>> GetDoctors()
        {
            var doctors = _doctorService.GetAll()
                .Select(d => new DoctorResponseDTO
                {
                    DoctorId = d.DoctorId,
                    FullName = d.FullName,
                    SpecialtyName = d.Specialty?.SpecialtyName,
                    Phone = d.Phone,
                    Email = d.Email
                });

            return Ok(doctors);
        }

        // ======================== 2️⃣ XEM CHI TIẾT BÁC SĨ ========================
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        [HttpGet("{id}")]
        public ActionResult<DoctorResponseDTO> GetDoctor(int id)
        {
            var doctor = _doctorService.GetById(id);
            if (doctor == null)
                return NotFound("Không tìm thấy bác sĩ.");

            var dto = new DoctorResponseDTO
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.FullName,
                SpecialtyName = doctor.Specialty?.SpecialtyName,
                Phone = doctor.Phone,
                Email = doctor.Email
            };

            return Ok(dto);
        }

        // ======================== 3️⃣ ADMIN THÊM BÁC SĨ MỚI ========================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateDoctor([FromBody] DoctorRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = new Doctor
            {
                FullName = dto.FullName,
                SpecialtyId = dto.SpecialtyId,
                Phone = dto.Phone,
                Email = dto.Email
            };

            _doctorService.Create(doctor);
            return Ok(new { message = "Thêm bác sĩ thành công!", doctorId = doctor.DoctorId });
        }

        // ======================== 4️⃣ ADMIN CẬP NHẬT THÔNG TIN ========================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateDoctor(int id, [FromBody] DoctorRequestDTO dto)
        {
            var existing = _doctorService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy bác sĩ.");

            existing.FullName = dto.FullName;
            existing.SpecialtyId = dto.SpecialtyId;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;

            _doctorService.Update(existing);
            return Ok(new { message = "Cập nhật bác sĩ thành công!" });
        }

        // ======================== 5️⃣ ADMIN XÓA BÁC SĨ ========================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteDoctor(int id)
        {
            var doctor = _doctorService.GetById(id);
            if (doctor == null)
                return NotFound("Không tìm thấy bác sĩ.");

            _doctorService.Delete(id);
            return Ok(new { message = "Xóa bác sĩ thành công!" });
        }
    }
}
