using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhongKham.API.Models.DTOs;
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

        // ==================== 1️⃣ LẤY DANH SÁCH ====================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _patientService.GetAll()
                .Select(p => new PatientDTO
                {
                    PatientId = p.PatientId,
                    FullName = p.FullName,
                    Dob = p.Dob,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    MedicalHistory = p.MedicalHistory,
                    AccountId = p.AccountId
                });

            return Ok(list);
        }

        // ==================== 2️⃣ XEM CHI TIẾT ====================
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _patientService.GetById(id);
            if (p == null)
                return NotFound("Không tìm thấy bệnh nhân.");

            var dto = new PatientDTO
            {
                PatientId = p.PatientId,
                FullName = p.FullName,
                Dob = p.Dob,
                Gender = p.Gender,
                Phone = p.Phone,
                Email = p.Email,
                Address = p.Address,
                MedicalHistory = p.MedicalHistory,
                AccountId = p.AccountId
            };

            return Ok(dto);
        }

        // ==================== 3️⃣ ADMIN / LỄ TÂN THÊM BỆNH NHÂN ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        public IActionResult Create([FromBody] PatientDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = new Patient
            {
                FullName = dto.FullName!,
                Dob = dto.Dob ?? DateTime.Now, // nếu chưa nhập, mặc định ngày hiện tại
                Gender = dto.Gender,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                MedicalHistory = dto.MedicalHistory,
                AccountId = dto.AccountId
            };
            _patientService.Create(patient);
            return Ok(new { message = "Thêm bệnh nhân thành công!", patientId = patient.PatientId });
        }

        // ==================== 4️⃣ CẬP NHẬT ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] PatientDTO dto)
        {
            var existing = _patientService.GetById(id);
            if (existing == null)
                return NotFound("Không tìm thấy bệnh nhân.");

            existing.FullName = dto.FullName!;
            existing.Dob = dto.Dob;
            existing.Gender = dto.Gender;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.MedicalHistory = dto.MedicalHistory;

            _patientService.Update(existing);
            return Ok(new { message = "Cập nhật bệnh nhân thành công!" });
        }

        // ==================== 5️⃣ XÓA ====================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _patientService.Delete(id);
            return Ok(new { message = "Xóa bệnh nhân thành công!" });
        }

        // ==================== 6️⃣ BỆNH NHÂN XEM HỒ SƠ CỦA MÌNH ====================
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

            var dto = new PatientDTO
            {
                PatientId = patient.PatientId,
                FullName = patient.FullName,
                Dob = patient.Dob,
                Gender = patient.Gender,
                Phone = patient.Phone,
                Email = patient.Email,
                Address = patient.Address,
                MedicalHistory = patient.MedicalHistory,
                AccountId = patient.AccountId
            };

            return Ok(dto);
        }
    }
}