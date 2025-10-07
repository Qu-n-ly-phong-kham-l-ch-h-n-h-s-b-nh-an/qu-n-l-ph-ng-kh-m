using System;
using System.ComponentModel.DataAnnotations;

namespace PhongKham.API.Models.DTOs
{
    public class AuditLogRequestDTO
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [StringLength(200)]
        public string Action { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = null!;

        public int? RecordId { get; set; }
    }

    public class AuditLogResponseDTO
    {
        public int LogId { get; set; }
        public int? AccountId { get; set; }
        public string? Action { get; set; }
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime? AccessTime { get; set; }
    }
}