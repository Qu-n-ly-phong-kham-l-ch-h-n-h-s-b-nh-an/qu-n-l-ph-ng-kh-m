using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    /// <summary>
    /// Model chính, ánh xạ bảng Accounts.
    /// </summary>
    public class Account
    {
        public int AccountID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho request đăng ký.
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; set; } = "Patient";
    }

    /// <summary>
    /// DTO cho request đăng nhập thông thường.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho request đăng nhập theo vai trò.
    /// </summary>
    public class RoleLoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò không được để trống.")]
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho request đổi mật khẩu.
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}