using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;
        private readonly AccountService _accountService;

        public PatientsController(PatientService patientService, AccountService accountService)
        {
            _patientService = patientService;
            _accountService = accountService;
        }

        // ==================== LẤY DANH SÁCH ====================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_patientService.GetAll());
        }

        // ==================== LẤY CHI TIẾT ====================
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var patient = _patientService.GetById(id);
            if (patient == null)
                return NotFound("Không tìm thấy bệnh nhân");
            return Ok(patient);
        }

        // ==================== THÊM MỚI ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] Patient patient)
        {
            if (patient == null)
                return BadRequest("Dữ liệu không hợp lệ");

            _patientService.Create(patient);
            return Ok(new { message = "Thêm bệnh nhân thành công", patient });
        }

        // ==================== CẬP NHẬT ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Patient patient)
        {
            if (id != patient.PatientId)
                return BadRequest("ID không khớp");

            _patientService.Update(patient);
            return Ok(new { message = "Cập nhật thành công", patient });
        }

        // ==================== XÓA ====================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _patientService.Delete(id);
            return NoContent();
        }

        // ==================== BỆNH NHÂN XEM HỒ SƠ CỦA MÌNH ====================
        [Authorize(Roles = "Patient")]
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var account = _accountService.GetByUsername(username);
            if (account == null)
                return NotFound("Không tìm thấy tài khoản.");

            var patient = _patientService.GetByAccountId(account.AccountId);
            if (patient == null)
                return NotFound("Không tìm thấy hồ sơ bệnh nhân.");

            return Ok(patient);
        }
    }
}
