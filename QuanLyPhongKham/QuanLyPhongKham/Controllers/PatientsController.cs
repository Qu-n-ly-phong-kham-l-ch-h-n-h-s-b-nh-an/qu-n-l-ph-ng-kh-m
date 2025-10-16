// File: Controllers/PatientsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;
using System.Security.Claims;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientBLL _bus;

        public PatientsController(PatientBLL bus)
        {
            _bus = bus;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public IActionResult GetAll()
        {
            var list = _bus.GetAll();
            return Ok(list);
        }

        // ✅ HOÀN THIỆN: Cho phép Bệnh nhân xem hồ sơ của chính mình
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Patient")]
        public IActionResult GetById(int id)
        {
            // Logic kiểm tra quyền sở hữu
            if (User.IsInRole("Patient"))
            {
                var patientProfile = _bus.GetByAccountId(int.Parse(User.FindFirstValue("AccountID")!));
                if (patientProfile == null || patientProfile.PatientID != id)
                {
                    return Forbid(); // Cấm truy cập nếu không phải hồ sơ của mình
                }
            }

            var patient = _bus.GetById(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create([FromBody] PatientCreateRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
            var newId = _bus.Create(req);
            var newPatient = _bus.GetById(newId);
            // Trả về đối tượng vừa tạo, là một thực hành tốt
            return CreatedAtAction(nameof(GetById), new { id = newId }, newPatient);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Update(int id, [FromBody] PatientUpdateRequest req)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound();
            return Ok(new { message = "Cập nhật hồ sơ bệnh nhân thành công." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            // ✅ DỌN DẸP: Xóa try-catch
            var ok = _bus.Delete(id);
            if (!ok) return NotFound();
            return NoContent(); // Trả về 204 No Content cho thao tác xóa thành công
        }
    }
}