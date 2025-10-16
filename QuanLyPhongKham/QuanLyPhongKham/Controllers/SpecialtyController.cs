// File: Controllers/SpecialtiesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize] // Yêu cầu xác thực cho tất cả
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtiesController : ControllerBase
    {
        private readonly SpecialtyBLL _bus;
        public SpecialtiesController(SpecialtyBLL bus)
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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public IActionResult GetById(int id)
        {
            var specialty = _bus.GetById(id);
            if (specialty == null) return NotFound();
            return Ok(specialty);
        }

        // ✅ DỌN DẸP: Xóa try-catch, Middleware sẽ tự động xử lý lỗi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] SpecialtyRequest req)
        {
            var newId = _bus.Create(req);
            var newSpecialty = _bus.GetById(newId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, newSpecialty);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id, [FromBody] SpecialtyRequest req)
        {
            var ok = _bus.Update(id, req);
            if (!ok) return NotFound(new { message = "Không tìm thấy chuyên khoa để cập nhật." });
            return Ok(new { message = "Cập nhật chuyên khoa thành công." });
        }

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