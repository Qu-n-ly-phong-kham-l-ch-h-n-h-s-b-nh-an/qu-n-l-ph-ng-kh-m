// File: Controllers/AccountsController.cs
using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountBLL _bus;

        // ✅ SỬA LỖI: Sử dụng Dependency Injection đúng cách, inject BLL thay vì IConfiguration
        public AccountsController(AccountBLL bus)
        {
            _bus = bus;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _bus.GetAll();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var currentUserIdStr = User.FindFirstValue("AccountID");

            if (!User.IsInRole("Admin") && (currentUserIdStr == null || id != int.Parse(currentUserIdStr)))
            {
                return Forbid();
            }

            var acc = _bus.GetById(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            // ✅ DỌN DẸP: Chỉ cần try-catch cho các lỗi validation cụ thể nếu cần,
            // còn lại Middleware sẽ tự động bắt ApplicationException (lỗi nghiệp vụ) và Exception (lỗi hệ thống)
            try
            {
                var newId = _bus.Register(req.Username, req.Password, req.Role ?? "Patient");
                var newAccount = new { AccountID = newId, Username = req.Username };
                return CreatedAtAction(nameof(GetById), new { id = newId }, newAccount);
            }
            catch (ArgumentException ex)
            {
                // Trả về 400 Bad Request cho các lỗi dữ liệu đầu vào không hợp lệ
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var acc = _bus.Authenticate(req.Username, req.Password);
            if (acc == null) return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu." });

            var token = _bus.GenerateJwtToken(acc);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                user = new { acc.AccountID, acc.Username, acc.Role, acc.IsActive }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Account acc)
        {
            if (id != acc.AccountID) return BadRequest();

            var ok = _bus.UpdateInfo(acc);
            if (!ok) return NotFound();
            return Ok(new { message = "Cập nhật thông tin thành công." });
            // Không cần try-catch vì Middleware đã xử lý
        }

        [HttpPatch("{id}/password")]
        public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest req)
        {
            var currentUserIdStr = User.FindFirstValue("AccountID");

            if (!User.IsInRole("Admin") && (currentUserIdStr == null || id != int.Parse(currentUserIdStr)))
            {
                return Forbid();
            }

            try
            {
                var ok = _bus.ChangePassword(id, req.NewPassword);
                if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản." });
                return Ok(new { message = "Đổi mật khẩu thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ok = _bus.Delete(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy tài khoản." });
            return NoContent();
            // Không cần try-catch vì Middleware đã xử lý
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/active")]
        public IActionResult SetActive(int id, [FromQuery] bool value)
        {
            var ok = _bus.SetActive(id, value);
            if (!ok) return NotFound();
            return Ok(new { message = value ? "Kích hoạt thành công" : "Khoá tài khoản thành công." });
            // Không cần try-catch vì Middleware đã xử lý
        }
    }

    // Các DTOs được giữ nguyên, có thể đặt trong cùng file hoặc file riêng trong Models
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; } = "Patient";
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}