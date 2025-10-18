// File: Models/Drug.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamApi.Models
{
    // Model chính cho bảng Drugs
    public class Drug
    {
        public int DrugID { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedByAccountID { get; set; }
        public bool IsDeleted { get; set; }
    }

    // DTO để hiển thị báo cáo tồn kho (kết hợp thông tin thuốc và số lượng)
    public class DrugStockDTO
    {
        public int DrugID { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // DTO cho request tạo/cập nhật thuốc
    public class DrugRequest
    {
        [Required(ErrorMessage = "Tên thuốc không được để trống.")]
        [StringLength(100)]
        public string DrugName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Giá thuốc không được để trống.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá thuốc phải là số không âm.")]
        public decimal Price { get; set; }
    }

    // DTO cho request điều chỉnh tồn kho (Theo logic type: import/export)
    public class StockAdjustRequest
    {
        [Required]
        public int DrugID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là số dương.")] // Quantity luôn dương
        public int Quantity { get; set; }

        [Required]
        [RegularExpression("^(import|export)$", ErrorMessage = "Loại điều chỉnh phải là 'import' hoặc 'export'.")] // Chỉ chấp nhận 'import' hoặc 'export'
        public string Type { get; set; } = string.Empty; // "import" or "export"
    }
}