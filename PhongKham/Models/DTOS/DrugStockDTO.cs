using System;
using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class DrugStockDTO
    {
        public int StockId { get; set; }

        [Required]
        public int DrugId { get; set; }

        public string? DrugName { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}