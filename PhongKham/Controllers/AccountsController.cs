using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PhongKham.BLL.Service;
using PhongKham.DAL.Entities;

namespace PhongKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TokenService _tokenService;

        // ✅ Constructor duy nhất (đã gộp 2 cái thành 1)
        public AccountsController(AccountService accountService, TokenService tokenService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        // ======================= CRUD CƠ BẢN =======================

        // GET: api/accounts
        [HttpGet]
        public IActionResult GetAll()
        {
            var accounts = _accountService.GetAll();
            return Ok(accounts);
        }

        // GET: api/accounts/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var account = _accountService.GetById(id);
            if (account == null)
                return NotFound();
            return Ok(account);
        }

        // POST: api/accounts
        [HttpPost]
        public IActionResult Create([FromBody] Account account)
        {
            if (account == null)
                return BadRequest("Dữ liệu không hợp lệ");

            _accountService.Create(account);
            return Ok(account);
        }

        // PUT: api/accounts/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Account account)
        {
            if (id != account.AccountId)
                return BadRequest("ID không khớp");

            _accountService.Update(account);
            return Ok(account);
        }

        // DELETE: api/accounts/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _accountService.Delete(id);
            return NoContent();
        }

        // ======================= LOGIN & TOKEN =======================

        // POST: api/accounts/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] Account login)
        {
            if (login == null)
                return BadRequest("Thiếu thông tin đăng nhập");

            var user = _accountService.GetByUsername(login.Username);
            if (user == null || user.PasswordHash != login.PasswordHash)
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token, role = user.Role });
        }

        // ======================= TEST PHÂN QUYỀN =======================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("Xin chào Admin");
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor-only")]
        public IActionResult DoctorOnly()
        {
            return Ok("Xin chào Bác sĩ");
        }

        // ======================= ĐĂNG KÝ TÀI KHOẢN (ADMIN) =======================
        [Authorize(Roles = "Admin")] // chỉ Admin mới được phép tạo tài khoản mới
        [HttpPost("register")]
        public IActionResult Register([FromBody] Account account)
        {
            if (account == null)
                return BadRequest("Dữ liệu không hợp lệ");

            _accountService.Create(account);
            return Ok(new { message = "Tạo tài khoản thành công", account });
        }



    }
}