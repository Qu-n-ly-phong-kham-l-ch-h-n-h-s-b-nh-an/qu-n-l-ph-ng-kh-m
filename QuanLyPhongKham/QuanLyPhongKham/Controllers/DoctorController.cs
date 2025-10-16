// File: Controllers/DoctorsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize] // Yêu cầu xác thực cho tất cả API trong controller
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorBLL _bus;
        public DoctorsController(DoctorBLL bus)
        {
            _bus = bus;
        }

        // ✅ HOÀN THIỆN: Cho phép nhiều vai trò xem danh sách
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public IActionResult GetAll()
        {
            return Ok(_bus.GetAll());
        }

        // ✅ HOÀN THIỆN: Bổ sung API Get By ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public IActionResult GetById(int id)
        {
            var doctor = _bus.GetById(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] DoctorCreateRequest req)
        {
            var newId = _bus.Create(req);
            var newDoctor = _bus.GetById(newId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newDoctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] DoctorUpdateRequest req)
        {
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound();
            return Ok(new { message = "Cập nhật thông tin bác sĩ thành công." });
        }

        // ✅ HOÀN THIỆN: Bổ sung API Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var ok = _bus.Delete(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}