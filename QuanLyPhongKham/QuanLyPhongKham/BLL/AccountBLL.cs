using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System.IdentityModel.Tokens.Jwt; // Cần cài gói này
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace QuanLyPhongKhamApi.BLL
{
    public class AccountBLL
    {
        private readonly AccountDAL _dal;
        private readonly IConfiguration _config;

        public AccountBLL(IConfiguration config)
        {
            _dal = new AccountDAL(config);
            _config = config; // Lấy Configuration
        }

        // Lấy tất cả (Giữ nguyên)
        public List<Account> GetAll() => _dal.GetAll();

        // Lấy theo id (Giữ nguyên)
        public Account? GetById(int id) => _dal.GetById(id);

        // Đăng ký tài khoản (Giữ nguyên)
        public int Register(string username, string password, string role = "Patient")
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username không được để trống.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentException("Password không hợp lệ (ít nhất 6 ký tự).");

            var existing = _dal.GetByUsername(username);
            if (existing != null)
                throw new ApplicationException("Tên đăng nhập đã tồn tại.");

            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            int newId = _dal.Register(username, hash, role);
            if (newId <= 0)
                throw new ApplicationException("Đăng ký thất bại.");
            return newId;
        }

        // Authenticate (Giữ nguyên)
        public Account? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var acc = _dal.GetByUsername(username);
            if (acc == null) return null;
            if (!acc.IsActive) return null; // tài khoản bị khoá

            bool ok = BCrypt.Net.BCrypt.Verify(password, acc.PasswordHash);
            return ok ? acc : null;
        }

        // 🟢 Tạo JWT Token
        public string GenerateJwtToken(Account account)
        {
            // Đảm bảo các khóa JWT được định nghĩa trong appsettings.json
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim("AccountID", account.AccountID.ToString()), // Dùng để xác thực chủ tài khoản
                new Claim(ClaimTypes.Role, account.Role) // Dùng để phân quyền
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🟢 Cập nhật thông tin cơ bản (không có mật khẩu)
        public bool UpdateInfo(Account acc)
        {
            if (acc.AccountID <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.UpdateInfo(acc);
        }

        // 🟢 Đổi mật khẩu
        public bool ChangePassword(int id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("Password mới không hợp lệ (ít nhất 6 ký tự).");

            string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return _dal.UpdatePasswordHash(id, hash);
        }

        // Delete (Soft Delete - Giữ nguyên logic cũ, DAL đã sửa)
        public bool Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.Delete(id);
        }

        // SetActive (Giữ nguyên)
        public bool SetActive(int id, bool active)
        {
            if (id <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.SetActive(id, active);
        }
    }
}