using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class AuditLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("AccountID")]
    public int? AccountId { get; set; }

    [StringLength(200)]
    public string? Action { get; set; }

    [StringLength(100)]
    public string? TableName { get; set; }

    [Column("RecordID")]
    public int? RecordId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AccessTime { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("AuditLogs")]
    public virtual Account? Account { get; set; }
}
