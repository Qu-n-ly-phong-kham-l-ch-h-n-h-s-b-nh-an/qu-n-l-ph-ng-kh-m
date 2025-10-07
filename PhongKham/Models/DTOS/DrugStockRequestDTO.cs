using System;
using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class DrugStockRequestDTO
    {
        [Required]
        public int DrugId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }
    }

    public class DrugStockResponseDTO
    {
        public int StockId { get; set; }
        public int DrugId { get; set; }
        public string? DrugName { get; set; }
        public int? QuantityAvailable { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}