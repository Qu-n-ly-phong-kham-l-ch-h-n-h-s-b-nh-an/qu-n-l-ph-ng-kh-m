using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    // Model ánh xạ trực tiếp từ DB
    public class Patient
    {
        public int PatientID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
        public int? AccountID { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    // DTO dùng để tạo mới Hồ sơ Bệnh nhân (dùng cho Receptionist/Admin)
    public class PatientCreateRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }

        // Dùng khi tạo Account cho bệnh nhân ngay lập tức
        public string? AccountUsername { get; set; }
        public string? AccountPassword { get; set; }
    }

    // DTO dùng để cập nhật Hồ sơ Bệnh nhân
    public class PatientUpdateRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
    }
}
