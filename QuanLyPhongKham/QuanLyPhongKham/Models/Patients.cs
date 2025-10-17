using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    // Model ánh xạ trực tiếp từ DB (Không đổi)
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

    // DTO dùng để tạo mới Hồ sơ Bệnh nhân (Đã cập nhật)
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

        // --- Tùy chọn 1: Tạo tài khoản mới ---
        public string? AccountUsername { get; set; }
        public string? AccountPassword { get; set; }

        // --- Tùy chọn 2: Liên kết với tài khoản đã có ---
        public int? AccountID { get; set; } // <-- THÊM DÒNG NÀY
    }

    // DTO dùng để cập nhật Hồ sơ Bệnh nhân (Không đổi)
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

        // --- BỔ SUNG TRƯỜNG AccountID VÀO ĐÂY ---
        public int? AccountID { get; set; }
    }
}