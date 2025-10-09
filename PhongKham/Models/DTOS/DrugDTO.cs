using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class DrugDTO
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        public string DrugName { get; set; } = null!;

        [StringLength(50)]
        public string? Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá thuốc phải >= 0")]
        public decimal? Price { get; set; }
  
        public int DrugId { get; set; }

    }
}