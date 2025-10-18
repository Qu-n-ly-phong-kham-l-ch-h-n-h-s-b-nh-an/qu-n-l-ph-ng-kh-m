// File: Controllers/AccountsController.cs
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Collections.Generic; // Cần thiết cho Dictionary

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize] // Yêu cầu xác thực cho hầu hết các endpoint
    [ApiController]
    [Route("api/[controller]")] // Route chuẩn: /api/Accounts
    public class AccountsController : ControllerBase
    {
        private readonly AccountBLL _bus;

        public AccountsController(AccountBLL bus)
        {
            _bus = bus;
        }

        // GET /api/Accounts (Lấy tất cả tài khoản)
        [Authorize(Roles = "Admin,Receptionist")] // Admin hoặc Lễ tân có thể xem
        [HttpGet]
        public IActionResult GetAll() => Ok(_bus.GetAll());

        // GET /api/Accounts/{id} (Lấy tài khoản theo ID)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var currentUserIdStr = User.FindFirstValue("AccountID"); // Lấy AccountID từ token
            // Chỉ Admin hoặc chính chủ tài khoản mới được xem thông tin chi tiết
            if (!User.IsInRole("Admin") && (currentUserIdStr == null || !int.TryParse(currentUserIdStr, out int currentUserId) || id != currentUserId))
            {
                return Forbid(); // 403 Forbidden nếu không có quyền
            }
            var acc = _bus.GetById(id);
            if (acc == null) return NotFound(); // 404 Not Found
            return Ok(acc);
        }

        // POST /api/Accounts/register (Đăng ký tài khoản - Bệnh nhân)
        [AllowAnonymous] // Cho phép truy cập công khai
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            // Middleware sẽ tự bắt ArgumentException và ApplicationException từ BLL
            var newId = _bus.Register(req.Username, req.Password, req.Role ?? "Patient"); // Mặc định là Patient
            var newAccount = _bus.GetById(newId); // Lấy lại thông tin tài khoản vừa tạo
            if (newAccount == null)
            {
                // Trường hợp hiếm gặp: tạo thành công nhưng không lấy lại được
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin tài khoản vừa tạo." });
            }
            // Trả về 201 Created với thông tin tài khoản mới và đường dẫn đến tài khoản đó
            return CreatedAtAction(nameof(GetById), new { id = newId }, newAccount);
        }

        // --- LOẠI BỎ Endpoint /login cũ ---
        /*
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
             // Hàm Authenticate không còn tồn tại trong BLL mới
            // var acc = _bus.Authenticate(req.Username, req.Password);
             // ...
        }
        */

        // POST /api/Accounts/role-login (Đăng nhập theo vai trò - Dùng cho tất cả)
        [AllowAnonymous] // Cho phép truy cập công khai
        [HttpPost("role-login")]
        public IActionResult RoleLogin([FromBody] RoleLoginRequest req)
        {
            try
            {
                // Gọi hàm RoleLogin mới trong BLL, trả về { token, user }
                var result = _bus.RoleLogin(req.Username, req.Password, req.Role);
                return Ok(result); // Trả về object chứa token và user (có thể có patientId)
            }
            catch (ArgumentException ex) // Lỗi đầu vào (vd: thiếu username/pass)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ApplicationException ex) // Lỗi nghiệp vụ (vd: sai pass, sai role, tk bị khóa)
            {
                return Unauthorized(new { message = ex.Message }); // Trả về 401 Unauthorized
            }
            catch (Exception ex) // Lỗi không mong muốn khác
            {
                // Ghi log lỗi ở đây nếu cần
                Console.WriteLine($"Unexpected error during RoleLogin: {ex}"); // Log ra console để debug
                return StatusCode(500, new { message = "Lỗi hệ thống trong quá trình đăng nhập." });
            }
        }

        // PUT /api/Accounts/{id} (Cập nhật thông tin cơ bản - Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Account acc) // Nhận toàn bộ object Account để BLL xử lý
        {
            if (id != acc.AccountID) return BadRequest(new { message = "ID trong URL và body không khớp." });
            var ok = _bus.UpdateInfo(acc);
            if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản để cập nhật." });
            return Ok(new { message = "Cập nhật thông tin tài khoản thành công." });
        }

        // PATCH /api/Accounts/{id}/password (Đổi mật khẩu)
        [HttpPatch("{id}/password")]
        public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest req)
        {
            var currentUserIdStr = User.FindFirstValue("AccountID");
            // Chỉ Admin hoặc chính chủ tài khoản mới được đổi mật khẩu
            if (!User.IsInRole("Admin") && (currentUserIdStr == null || !int.TryParse(currentUserIdStr, out int currentUserId) || id != currentUserId))
            {
                return Forbid();
            }
            // Middleware sẽ bắt ArgumentException từ BLL
            var ok = _bus.ChangePassword(id, req.NewPassword);
            if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản." });
            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        // DELETE /api/Accounts/{id} (Xóa mềm - Khóa tài khoản - Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ok = _bus.Delete(id); // Hàm Delete trong BLL thực ra là gọi SetActive(false)
            if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản để khóa." });
            // Trả về 204 No Content là chuẩn cho Delete thành công
            return NoContent();
        }

        // PATCH /api/Accounts/{id}/active (Kích hoạt/Khóa tài khoản - Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/active")]
        public IActionResult SetActive(int id, [FromQuery] bool value)
        {
            var ok = _bus.SetActive(id, value);
            if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản." });
            return Ok(new { message = value ? "Kích hoạt tài khoản thành công." : "Khóa tài khoản thành công." });
        }
    }
}