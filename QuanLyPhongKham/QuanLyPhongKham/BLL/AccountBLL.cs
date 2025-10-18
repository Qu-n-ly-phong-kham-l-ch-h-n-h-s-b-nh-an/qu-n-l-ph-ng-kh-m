// File: BLL/AccountBLL.cs
using QuanLyPhongKhamApi.DAL;
using QuanLyPhongKhamApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic; // Cần thiết cho Dictionary

namespace QuanLyPhongKhamApi.BLL
{
    public class AccountBLL
    {
        private readonly AccountDAL _dal;
        private readonly PatientDAL _patientDal; // <-- BỔ SUNG: Inject PatientDAL
        private readonly IConfiguration _config;

        // <-- SỬA ĐỔI: Thêm PatientDAL vào constructor -->
        public AccountBLL(AccountDAL dal, PatientDAL patientDal, IConfiguration config)
        {
            _dal = dal;
            _patientDal = patientDal; // <-- BỔ SUNG
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
            {
                throw new ApplicationException("Đăng ký tài khoản thất bại.");
            }
            return newId;
        }

        // === HÀM LOGIN ĐÃ CẬP NHẬT ĐỂ TRẢ VỀ patientId ===
        public object RoleLogin(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Tên đăng nhập và mật khẩu không được để trống.");

            var account = _dal.GetByUsername(username);

            if (account == null || !account.IsActive)
                throw new ApplicationException("Tài khoản không tồn tại hoặc đã bị khóa.");

            // So sánh Role không phân biệt hoa thường
            if (!account.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException($"Vai trò đăng nhập không đúng. Tài khoản này thuộc vai trò '{account.Role}'.");

            if (!BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
                throw new ApplicationException("Mật khẩu không chính xác.");

            // Tạo đối tượng user trả về (dùng Dictionary để linh hoạt)
            var userPayload = new Dictionary<string, object>
            {
                { "accountId", account.AccountID },
                { "username", account.Username },
                { "role", account.Role }
            };

            // *** Lấy patientId nếu là Patient ***
            if (account.Role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patientProfile = _patientDal.GetByAccountId(account.AccountID); // Gọi DAL lấy hồ sơ BN
                if (patientProfile != null)
                {
                    // Thêm patientId vào payload nếu tìm thấy
                    userPayload.Add("patientId", patientProfile.PatientID);
                }
                // Nếu không có patientProfile, frontend sẽ xử lý
            }

            // Tạo token JWT
            var token = GenerateJwtToken(account);

            // Trả về cả token và user payload
            return new { token, user = userPayload };
        }

        // Hàm GenerateJwtToken (giữ cấu trúc Claim[] như bạn cung cấp, bổ sung claim chuẩn)
        public string GenerateJwtToken(Account account)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] // Sử dụng mảng Claim[] như code gốc của bạn
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Username), // Claim chuẩn cho Subject (Username)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Claim chuẩn cho ID Token duy nhất
                new Claim("AccountID", account.AccountID.ToString()), // Claim tùy chỉnh AccountID
                new Claim(ClaimTypes.Role, account.Role) // Claim chuẩn cho Role
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Có thể cấu hình thời gian hết hạn
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool UpdateInfo(Account acc)
        {
            if (acc.AccountID <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            var currentAcc = _dal.GetById(acc.AccountID);
            if (currentAcc == null) return false;
            acc.PasswordHash = currentAcc.PasswordHash;
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
            return SetActive(id, false);
        }

        public bool SetActive(int id, bool active)
        {
            if (id <= 0) throw new ArgumentException("AccountID không hợp lệ.");
            return _dal.SetActive(id, active);
        }
    }
}