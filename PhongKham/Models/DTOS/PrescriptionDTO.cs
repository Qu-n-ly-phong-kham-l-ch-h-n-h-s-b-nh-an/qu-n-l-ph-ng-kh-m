using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class PrescriptionDTO
    {
        public int PrescriptionId { get; set; } // Khi tạo mới có thể = 0

        [Required(ErrorMessage = "EncounterId là bắt buộc")]
        public int EncounterId { get; set; }

        [Required(ErrorMessage = "DrugId là bắt buộc")]
        public int DrugId { get; set; }

        public string? DrugName { get; set; } // tự map khi GET

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng thuốc phải > 0")]
        public int Quantity { get; set; }

        public string? Usage { get; set; }
    }
}