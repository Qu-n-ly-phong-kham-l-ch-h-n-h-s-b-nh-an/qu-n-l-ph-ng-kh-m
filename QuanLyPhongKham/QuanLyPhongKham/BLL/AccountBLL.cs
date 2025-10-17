using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System.IdentityModel.Tokens.Jwt;
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

        public AccountBLL(AccountDAL dal, IConfiguration config)
        {
            _dal = dal;
            _config = config;
        }

        public List<Account> GetAll() => _dal.GetAll();

        public Account? GetById(int id) => _dal.GetById(id);

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

        public Account? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var acc = _dal.GetByUsername(username);
            if (acc == null || !acc.IsActive) return null;

            bool ok = BCrypt.Net.BCrypt.Verify(password, acc.PasswordHash);
            return ok ? acc : null;
        }

        public Account? AuthenticateWithRole(string username, string password, string role)
        {
            var acc = Authenticate(username, password);
            if (acc != null && acc.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
            {
                return acc;
            }
            return null;
        }

        public string GenerateJwtToken(Account account)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim("AccountID", account.AccountID.ToString()),
                new Claim(ClaimTypes.Role, account.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool UpdateInfo(Account acc)
        {
            if (acc.AccountID <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.UpdateInfo(acc);
        }

        public bool ChangePassword(int id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("Password mới không hợp lệ (ít nhất 6 ký tự).");

            string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return _dal.UpdatePasswordHash(id, hash);
        }

        public bool Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.Delete(id);
        }

        public bool SetActive(int id, bool active)
        {
            if (id <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.SetActive(id, active);
        }
    }
}