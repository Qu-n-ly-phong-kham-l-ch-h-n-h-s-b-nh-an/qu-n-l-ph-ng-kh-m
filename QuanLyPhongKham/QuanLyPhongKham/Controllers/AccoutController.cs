using Microsoft.AspNetCore.Mvc;
using QuanLyPhongKhamApi.BLL;
using QuanLyPhongKhamApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;

namespace QuanLyPhongKhamApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountBLL _bus;

        public AccountsController(AccountBLL bus)
        {
            _bus = bus;
        }

        // ✅ SỬA LẠI: Bổ sung vai trò "Receptionist"
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet]
        public IActionResult GetAll() => Ok(_bus.GetAll());

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
            try
            {
                var newId = _bus.Register(req.Username, req.Password, req.Role ?? "Patient");
                var newAccount = _bus.GetById(newId);
                return CreatedAtAction(nameof(GetById), new { id = newId }, newAccount);
            }
            catch (ArgumentException ex)
            {
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
            return Ok(new { message = "Đăng nhập thành công", token, user = new { acc.AccountID, acc.Username, acc.Role, acc.IsActive } });
        }

        [AllowAnonymous]
        [HttpPost("role-login")]
        public IActionResult RoleLogin([FromBody] RoleLoginRequest req)
        {
            var acc = _bus.AuthenticateWithRole(req.Username, req.Password, req.Role);
            if (acc == null)
            {
                return Unauthorized(new { message = "Sai tên đăng nhập, mật khẩu, hoặc vai trò không đúng." });
            }
            var token = _bus.GenerateJwtToken(acc);
            return Ok(new { message = "Đăng nhập thành công", token, user = new { acc.AccountID, acc.Username, acc.Role, acc.IsActive } });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Account acc)
        {
            if (id != acc.AccountID) return BadRequest();
            var ok = _bus.UpdateInfo(acc);
            if (!ok) return NotFound();
            return Ok(new { message = "Cập nhật thành công." });
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
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/active")]
        public IActionResult SetActive(int id, [FromQuery] bool value)
        {
            var ok = _bus.SetActive(id, value);
            if (!ok) return NotFound();
            return Ok(new { message = value ? "Kích hoạt thành công" : "Khóa tài khoản thành công." });
        }
    }
}