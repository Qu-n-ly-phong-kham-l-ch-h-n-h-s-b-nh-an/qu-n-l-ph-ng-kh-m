using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class SpecialtyDTO
    {
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "Tên chuyên khoa không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên chuyên khoa không vượt quá 100 ký tự.")]
        public string SpecialtyName { get; set; } = string.Empty;
    }
}