// File: Models/Doctor.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    /// <summary>
    /// Model ánh xạ trực tiếp tới bảng Doctors trong CSDL.
    /// </summary>
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? SpecialtyID { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? AccountID { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO để hiển thị thông tin bác sĩ một cách đầy đủ hơn.
    /// </summary>
    public class DoctorDTO : Doctor
    {
        public string? SpecialtyName { get; set; }
        public string? Username { get; set; }
    }

    /// <summary>
    /// DTO dùng để tạo mới một hồ sơ bác sĩ.
    /// Bao gồm cả thông tin để tạo tài khoản đăng nhập cho bác sĩ đó.
    /// </summary>
    public class DoctorCreateRequest
    {
        [Required(ErrorMessage = "Họ tên bác sĩ là bắt buộc.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? SpecialtyID { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }

        // --- Thông tin tạo tài khoản ---
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        public string AccountUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string AccountPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO dùng để cập nhật thông tin của một bác sĩ.
    /// Không bao gồm thông tin tài khoản vì việc này được quản lý riêng.
    /// </summary>
    public class DoctorUpdateRequest
    {
        [Required(ErrorMessage = "Họ tên bác sĩ là bắt buộc.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? SpecialtyID { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}