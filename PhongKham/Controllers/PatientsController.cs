using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ Bắt buộc có token
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        // ==================== GET ALL ====================
        // ✅ Admin, Doctor, Lễ tân có thể xem danh sách bệnh nhân
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _patientService.GetAll();
            return Ok(list);
        }

        // ==================== GET BY ID ====================
        // ✅ Admin, Doctor, Patient có thể xem hồ sơ
        // (Tạm thời chưa kiểm tra đúng người vì chưa có liên kết Account)
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var patient = _patientService.GetById(id);
            if (patient == null)
                return NotFound("Không tìm thấy bệnh nhân");

            return Ok(patient);
        }

        // ==================== CREATE ====================
        // ✅ Lễ tân được phép thêm bệnh nhân mới
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] Patient patient)
        {
            if (patient == null)
                return BadRequest("Dữ liệu không hợp lệ");

            _patientService.Create(patient);
            return Ok(new { message = "Thêm bệnh nhân thành công", patient });
        }

        // ==================== UPDATE ====================
        // ✅ Admin hoặc Lễ tân được phép sửa thông tin
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Patient patient)
        {
            if (id != patient.PatientId)
                return BadRequest("ID không khớp");

            _patientService.Update(patient);
            return Ok(new { message = "Cập nhật thành công", patient });
        }

        // ==================== DELETE ====================
        // ✅ Chỉ Admin được phép xóa hồ sơ
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _patientService.Delete(id);
            return NoContent();
        }
    }
}
