// File: Models/Specialty.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    /// <summary>
    /// Model này ánh xạ trực tiếp tới bảng Specialties trong CSDL.
    /// </summary>
    public class Specialty
    {
        public int SpecialtyID { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// DTO (Data Transfer Object) dùng cho việc tạo mới hoặc cập nhật chuyên khoa.
    /// Chứa các validation rule để kiểm tra dữ liệu đầu vào.
    /// </summary>
    public class SpecialtyRequest
    {
        [Required(ErrorMessage = "Tên chuyên khoa không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên chuyên khoa không được vượt quá 100 ký tự.")]
        public string SpecialtyName { get; set; } = string.Empty;
    }
}