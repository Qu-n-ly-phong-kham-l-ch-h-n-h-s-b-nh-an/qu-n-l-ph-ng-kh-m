using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PhongKham.API.Models.DTOs;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;
using System.Drawing;

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

        // ==================== LẤY DANH SÁCH CÓ PHÂN TRANG ====================
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [HttpGet("paged")]
        public IActionResult GetPaged(
            string? keyword,
            string? sortBy = "PatientId",
            string? order = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var result = _patientService.GetPaged(keyword, sortBy, order, page, pageSize);
            return Ok(result);
        }

        // ==================== 7️⃣ XUẤT DANH SÁCH BỆNH NHÂN RA EXCEL ====================
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("export")]
        public IActionResult ExportToExcel(string? keyword = null)
        {
            var list = _patientService.GetAll();

            // 🔍 Lọc nếu có keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(p =>
                    p.FullName.Contains(keyword) ||
                    p.Phone.Contains(keyword) ||
                    p.Email.Contains(keyword));
            }

            // ⚙️ Cấu hình EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Danh sách bệnh nhân");

            // 📋 Tiêu đề cột
            ws.Cells["A1"].Value = "Mã BN";
            ws.Cells["B1"].Value = "Họ và tên";
            ws.Cells["C1"].Value = "Ngày sinh";
            ws.Cells["D1"].Value = "Giới tính";
            ws.Cells["E1"].Value = "SĐT";
            ws.Cells["F1"].Value = "Email";
            ws.Cells["G1"].Value = "Địa chỉ";
            ws.Cells["H1"].Value = "Tiền sử bệnh";

            using (var range = ws.Cells["A1:H1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // 🧾 Dữ liệu
            int row = 2;
            foreach (var p in list)
            {
                ws.Cells[row, 1].Value = p.PatientId;
                ws.Cells[row, 2].Value = p.FullName;
                ws.Cells[row, 3].Value = p.Dob?.ToString("dd/MM/yyyy");
                ws.Cells[row, 4].Value = p.Gender;
                ws.Cells[row, 5].Value = p.Phone;
                ws.Cells[row, 6].Value = p.Email;
                ws.Cells[row, 7].Value = p.Address;
                ws.Cells[row, 8].Value = p.MedicalHistory;
                row++;
            }

            ws.Cells.AutoFitColumns();

            // 📦 Xuất file
            var stream = new MemoryStream(package.GetAsByteArray());
            string fileName = $"DanhSachBenhNhan_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }


    }
}