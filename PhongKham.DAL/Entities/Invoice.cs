using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhongKham.DAL.Entities;

public partial class Invoice
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("PatientID")]
    public int PatientId { get; set; }

    [Column("EncounterID")]
    public int EncounterId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("EncounterId")]
    [InverseProperty("Invoices")]
    public virtual Encounter Encounter { get; set; } = null!;

    [ForeignKey("PatientId")]
    [InverseProperty("Invoices")]
    public virtual Patient Patient { get; set; } = null!;
}
