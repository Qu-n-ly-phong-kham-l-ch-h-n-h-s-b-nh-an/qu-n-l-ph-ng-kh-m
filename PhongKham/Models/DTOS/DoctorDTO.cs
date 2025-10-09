using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Tên bác sĩ không được để trống.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cần chọn chuyên khoa.")]
        public int? SpecialtyId { get; set; }

        public string? SpecialtyName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }
    }
}