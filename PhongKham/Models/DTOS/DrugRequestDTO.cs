using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class DrugRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string DrugName { get; set; } = null!;

        [StringLength(50)]
        public string? Unit { get; set; }

        public decimal? Price { get; set; }
    }
}
